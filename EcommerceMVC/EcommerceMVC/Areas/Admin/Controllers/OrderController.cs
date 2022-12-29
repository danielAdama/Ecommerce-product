using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Data.DTO;
using Ecommerce.Infrastructure.Services.Interface;
using Ecommerce.Infrastructure.Utilities;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
		public async Task<IActionResult> CancelOrder(OrderDTO orderDTO, CancellationToken cancellationToken)
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
				var claimsIdentity = (ClaimsIdentity)User.Identity;
				var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

				orderHeaders = await _context.OrderHeaders.Where(x => x.EcommerceUserId.Equals(
					Convert.ToInt64(claim.Value)))
					.Include(u => u.EcommerceUser)
					.ToListAsync();
				//if ()
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