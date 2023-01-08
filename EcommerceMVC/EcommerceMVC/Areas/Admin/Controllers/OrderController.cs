using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Data.DTO;
using Ecommerce.Infrastructure.Services.Interface;
using Ecommerce.Infrastructure.Utilities;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading;

namespace EcommerceMVC.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public class OrderController : Controller
	{
		private readonly EcommerceDbContext _context;
		public OrderController(EcommerceDbContext context)
		{
			_context = context;
		}
		public IActionResult Index()
		{
			return View();
		}
		public async Task<IActionResult> Details(long orderid, OrderDTO orderDTO)
		{
			orderDTO = new OrderDTO()
			{
				OrderHeader = await _context.OrderHeaders.Include(u => u.EcommerceUser).FirstOrDefaultAsync(
					x => x.Id.Equals(orderid)),
				OrderDetail = await _context.OrderDetails.Where(x => x.OrderId.Equals(orderid)).Include(u => u.Product)
					.ToListAsync(),

			};
			return View(orderDTO);
		}
		
		[HttpPost]
		[ActionName("Details")]
		public async Task<IActionResult> DetailsPayNow(OrderDTO orderDTO, CancellationToken cancellationToken)
		{
			orderDTO.OrderHeader = await _context.OrderHeaders.Include(u => u.EcommerceUser).FirstOrDefaultAsync(
					x => x.Id.Equals(orderDTO.OrderHeader.Id));
			orderDTO.OrderDetail = await _context.OrderDetails.Where(x => x.OrderId.Equals(orderDTO.OrderHeader.Id))
				.Include(u => u.Product)
				.ToListAsync(cancellationToken);

			//stripe settings 
			var domain = "https://localhost:44392/";
			var options = new SessionCreateOptions
			{
				PaymentMethodTypes = new List<string>
					{
					  "card",
					},
				LineItems = new List<SessionLineItemOptions>(),
				Mode = "payment",
				SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderid={orderDTO.OrderHeader.Id}",
				CancelUrl = domain + $"admin/order/details?orderid={orderDTO.OrderHeader.Id}",
			};

			foreach (var item in orderDTO.OrderDetail)
			{

				var sessionLineItem = new SessionLineItemOptions
				{
					PriceData = new SessionLineItemPriceDataOptions
					{
						/* Convert Naira to dollars inorder to use stripe */
						UnitAmount = ((Convert.ToInt64(item.Price / 446.63) * 100) / item.Count),// Convert price from cent 20.00 -> 2000
						Currency = "usd",
						ProductData = new SessionLineItemPriceDataProductDataOptions
						{
							Name = item.Product.Name
						},

					},
					Quantity = item.Count,
				};
				options.LineItems.Add(sessionLineItem);
			}

			var service = new SessionService();
			Session session = await service.CreateAsync(options);
			UpdateStripePaymentId(orderDTO.OrderHeader.Id, session.Id, session.PaymentIntentId);
			await _context.SaveChangesAsync(cancellationToken);
			Response.Headers.Add("Location", session.Url);
			return new StatusCodeResult(303);
		}

		public async Task<IActionResult> PaymentConfirmation(long orderHeaderid, CancellationToken cancellationToken)
		{
			int idint = Convert.ToUInt16(orderHeaderid);
			OrderHeader orderHeader = await _context.OrderHeaders.FirstOrDefaultAsync(x => x.Id.Equals(orderHeaderid));
			if (orderHeader.PaymentStatus == Constants.PaymentStatusDelayedPayment)
			{
				var service = new SessionService();
				Session session = service.Get(orderHeader.SessionId);
				/* Check the Stripe status */
				if (session.PaymentStatus.ToLower().Equals("paid"))
				{
					UpdateStripePaymentId(orderHeaderid, orderHeader.SessionId, session.PaymentIntentId);
					UpdateStatus(orderHeaderid, orderHeader.OrderStatus, Constants.PaymentStatusApproved);
					await _context.SaveChangesAsync(cancellationToken);
				}
			}
			return View(idint);
		}

		[HttpPost]
		[Authorize(Roles =Constants.RoleAdmin+","+Constants.RoleEmployee)]
		public async Task<IActionResult> UpdateOrderDetail(OrderDTO orderDTO, CancellationToken cancellationToken)
		{
			var order = await _context.OrderHeaders.FirstOrDefaultAsync(x => x.Id.Equals(orderDTO.OrderHeader.Id), cancellationToken);
			using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
			try
			{
				order.Name = orderDTO.OrderHeader.Name;
				order.PhoneNumber = orderDTO.OrderHeader.PhoneNumber;
				order.StreetAddress = orderDTO.OrderHeader.StreetAddress;
				order.City = orderDTO.OrderHeader.City;
				order.State = orderDTO.OrderHeader.State;
				order.PostalCode = orderDTO.OrderHeader.PostalCode;

				if (orderDTO.OrderHeader.Carrier != null)
				{
					order.Carrier = orderDTO.OrderHeader.Carrier;
				}
				if (orderDTO.OrderHeader.TrackingNumber != null)
				{
					order.TrackingNumber = orderDTO.OrderHeader.TrackingNumber;
				}
				await _context.SaveChangesAsync(cancellationToken);
				await transaction.CommitAsync(cancellationToken);
				TempData["success"] = "Order Details updated successfully";
				return RedirectToAction(nameof(Details), "Order", new { orderid = order.Id });
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(cancellationToken);
				TempData["errorMessage"] = ex.Message;
			}
			return View(orderDTO);
		}

		[HttpPost]
		[Authorize(Roles = Constants.RoleAdmin + "," + Constants.RoleEmployee)]
		public async Task<IActionResult> StartProcessing(OrderDTO orderDTO, CancellationToken cancellationToken)
		{
			using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
			try
			{
				UpdateStatus(orderDTO.OrderHeader.Id, Constants.StatusInProcess);
				await _context.SaveChangesAsync(cancellationToken);
				await transaction.CommitAsync(cancellationToken);
				TempData["success"] = "Order Status updated successfully";
				return RedirectToAction(nameof(Details), "Order", new { orderid = orderDTO.OrderHeader.Id });
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(cancellationToken);
				TempData["errorMessage"] = ex.Message;
			}
			return View(orderDTO);
		}

		[HttpPost]
		[Authorize(Roles = Constants.RoleAdmin + "," + Constants.RoleEmployee)]
		public async Task<IActionResult> ShipOrder(OrderDTO orderDTO, CancellationToken cancellationToken)
		{
			var orderHeader = await _context.OrderHeaders.FirstOrDefaultAsync(
				x => x.Id.Equals(orderDTO.OrderHeader.Id), cancellationToken);
			using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
			try
			{
				orderHeader.TrackingNumber = orderDTO.OrderHeader.TrackingNumber;
				orderHeader.Carrier = orderDTO.OrderHeader.Carrier;
				orderHeader.OrderStatus = Constants.StatusShipped;
				orderHeader.ShippingDate = DateTimeOffset.UtcNow;
				if(orderHeader.PaymentStatus == Constants.PaymentStatusDelayedPayment)
				{
					orderHeader.PaymentDueDate = DateTimeOffset.UtcNow.AddDays(30);
				}
				//_context.OrderHeaders.Update(orderHeader);
				await _context.SaveChangesAsync(cancellationToken);
				await transaction.CommitAsync(cancellationToken);
				TempData["success"] = "Order Shipped successfully";
				return RedirectToAction(nameof(Details), "Order", new { orderid = orderDTO.OrderHeader.Id });
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(cancellationToken);
				TempData["errorMessage"] = ex.Message;
			}
			return View(orderDTO);
		}
		[HttpPost]
		[Authorize(Roles = Constants.RoleAdmin + "," + Constants.RoleEmployee)]
		public async Task<IActionResult> CancelOrder(OrderDTO orderDTO, CancellationToken cancellationToken)
		{
			var orderHeader = await _context.OrderHeaders.FirstOrDefaultAsync(
				x => x.Id.Equals(orderDTO.OrderHeader.Id), cancellationToken);
			using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
			try
			{
				/* When order is cancelled refund the customer, if payed */
				if (orderHeader.PaymentStatus == Constants.PaymentStatusApproved)
				{
					var options = new RefundCreateOptions
					{
						Reason = RefundReasons.RequestedByCustomer,
						PaymentIntent = orderHeader.PaymentIntentId,
					};
					var service = new RefundService();
					Refund refund = await service.CreateAsync(options);
					UpdateStatus(orderHeader.Id, Constants.StatusCancelled, Constants.StatusRefunded);
				}
				if (orderHeader.PaymentStatus != Constants.PaymentStatusApproved)
				{
					UpdateStatus(orderHeader.Id, Constants.StatusCancelled, Constants.StatusCancelled);
				}
				await _context.SaveChangesAsync(cancellationToken);
				await transaction.CommitAsync(cancellationToken);
				TempData["success"] = "Order Cancelled successfully";
				return RedirectToAction(nameof(Details), "Order", new { orderid = orderDTO.OrderHeader.Id });
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(cancellationToken);
				TempData["errorMessage"] = ex.Message;
			}
			return View(orderDTO);
		}

		private void UpdateStripePaymentId(long id, string sessionId, string paymentIntentId)
		{
			var orders = _context.OrderHeaders.Find(id);

			orders.PaymentDate = DateTimeOffset.UtcNow;
			orders.SessionId = sessionId;
			orders.PaymentIntentId = paymentIntentId;
		}
		private void UpdateStatus(long id, string orderStatus, string? paymentStatus = null)
		{
			var orders = _context.OrderHeaders.Find(id);
			if (orders != null)
			{
				orders.OrderStatus = orderStatus;
				if (paymentStatus != null)
				{
					orders.PaymentStatus = paymentStatus;
				}
			}
		}

		#region API CALLS
		[HttpGet]
		public async Task<IActionResult> GetAll(string status)
		{

			IEnumerable<OrderHeader> orderHeaders;

			if (User.IsInRole(Constants.RoleAdmin) || User.IsInRole(Constants.RoleEmployee))
			{
				/* We need to load all the orders based on the user(email) */
				orderHeaders = await _context.OrderHeaders.Include(u => u.EcommerceUser).ToListAsync();
			}
			else
			{
				orderHeaders = await _context.OrderHeaders.Where(x => x.EcommerceUserId.Equals(
                    IdentityHelper.GetUserId(User.Identity)))
					.Include(u => u.EcommerceUser)
					.ToListAsync();
			}


			switch (status)
			{
				case "pending":
					orderHeaders = orderHeaders.Where(x => x.PaymentStatus == Constants.PaymentStatusDelayedPayment);
					break;

				case "inprocess":
					orderHeaders = orderHeaders.Where(x => x.OrderStatus == Constants.StatusInProcess); ;
					break;

				case "completed":
					orderHeaders = orderHeaders.Where(x => x.OrderStatus == Constants.StatusShipped); ;
					break;

				case "approved":
					orderHeaders = orderHeaders.Where(x => x.OrderStatus == Constants.StatusApproved); ;
					break;
			}

			return Json(new { data = orderHeaders });
		}
		#endregion
	}
}