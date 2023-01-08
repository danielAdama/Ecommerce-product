using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Data.DTO;
using Ecommerce.Infrastructure.Utilities;
using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using System.Security.Claims;

namespace EcommerceMVC.Areas.Customer.Controllers
{
	[Area("Customer")]
    [Authorize]
	public class CartController : Controller
    {
		private readonly EcommerceDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
#nullable disable
        public ShoppingCartDTO ShoppingCartDTO { get; set; }
		public int OrderTotal { get; set; }

		public CartController(EcommerceDbContext context, IHttpContextAccessor httpContextAccessor)
		{
			_context = context;
            _httpContextAccessor = httpContextAccessor;
        }

		public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
			/* Get all the products in a cart for a particular user */
			ShoppingCartDTO = new ShoppingCartDTO
			{
				ListCart = await _context.ShoppingCarts.Where(x => x.EcommerceUserId.Equals(Convert.ToInt64(claim.Value)))
				.Include(u => u.Product)
				.ToListAsync(cancellationToken),
				OrderHeader = new()
			};
			foreach(var cart in ShoppingCartDTO.ListCart)
			{
				var price = (cart.Count * cart.Product.Price);
				cart.Price = price;
				ShoppingCartDTO.OrderHeader.OrderTotal += price;
			}
			return View(ShoppingCartDTO);
        }

        public async Task<IActionResult> Summary(CancellationToken cancellationToken)
        {
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
			/* Get all the products in a cart for a particular user */
			ShoppingCartDTO = new ShoppingCartDTO
			{
				ListCart = await _context.ShoppingCarts.Where(x => x.EcommerceUserId.Equals(Convert.ToInt64(claim.Value)))
				.Include(u => u.Product)
				.ToListAsync(cancellationToken),
				OrderHeader = new()
			};

			/* Retrieve all the application order details for a logged in user */
			ShoppingCartDTO.OrderHeader.EcommerceUser = await _context.EcommerceUsers.FirstOrDefaultAsync(
				x => x.Id.Equals(Convert.ToInt64(claim.Value)), cancellationToken);

			ShoppingCartDTO.OrderHeader.Name = ShoppingCartDTO.OrderHeader.EcommerceUser.UserName;
			ShoppingCartDTO.OrderHeader.PhoneNumber = ShoppingCartDTO.OrderHeader.EcommerceUser.PhoneNumber;
			ShoppingCartDTO.OrderHeader.StreetAddress = ShoppingCartDTO.OrderHeader.EcommerceUser.StreetAddress;
			ShoppingCartDTO.OrderHeader.City = ShoppingCartDTO.OrderHeader.EcommerceUser.City;
			ShoppingCartDTO.OrderHeader.State = ShoppingCartDTO.OrderHeader.EcommerceUser.State;
			ShoppingCartDTO.OrderHeader.PostalCode = ShoppingCartDTO.OrderHeader.EcommerceUser.PostalCode;
            
			foreach (var cart in ShoppingCartDTO.ListCart)
			{
				var price = (cart.Count * cart.Product.Price);
				cart.Price = price;
				ShoppingCartDTO.OrderHeader.OrderTotal += price;
			}
			return View(ShoppingCartDTO);
        }

		[HttpPost]
		[ActionName("Summary")]
		[ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryPOST(ShoppingCartDTO ShoppingCartDTO, CancellationToken cancellationToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShoppingCartDTO.ListCart = await _context.ShoppingCarts.Where(x => x.EcommerceUserId.Equals(
				Convert.ToInt64(claim.Value)))
				.Include(u => u.Product)
				.ToListAsync(cancellationToken);

			ShoppingCartDTO.OrderHeader.OrderDate = DateTimeOffset.UtcNow;
			ShoppingCartDTO.OrderHeader.EcommerceUserId = Convert.ToInt64(claim.Value);

			foreach (var cart in ShoppingCartDTO.ListCart)
			{
				var price = (cart.Count * cart.Product.Price);
				cart.Price = price;
				ShoppingCartDTO.OrderHeader.OrderTotal += price;
			}

			/* If it's a company user allow them to make order without redirecting them to the 
			 * stripe, but company users are meant to pay between 30 days. */
			EcommerceUser ecommerceUser = await _context.EcommerceUsers.FindAsync(Convert.ToInt64(claim.Value));
			/* Flag as Delayed Payment and Approved order if it is a company user, otherwise flag as pending */
			if (ecommerceUser.CompanyId.GetValueOrDefault() != 0)
			{
				ShoppingCartDTO.OrderHeader.PaymentStatus = Constants.PaymentStatusDelayedPayment;
				ShoppingCartDTO.OrderHeader.OrderStatus = Constants.StatusApproved;
			}
			if (ecommerceUser.CompanyId.GetValueOrDefault() == 0)
			{
				ShoppingCartDTO.OrderHeader.PaymentStatus = Constants.PaymentStatusPending;
				ShoppingCartDTO.OrderHeader.OrderStatus = Constants.StatusPending;
			}

			await _context.OrderHeaders.AddAsync(ShoppingCartDTO.OrderHeader, cancellationToken);
			await _context.SaveChangesAsync(cancellationToken);

			foreach (var cart in ShoppingCartDTO.ListCart)
			{
				OrderDetail orderDetail = new()
				{
					ProductId = cart.ProductId,
					OrderId = ShoppingCartDTO.OrderHeader.Id,
					Price = cart.Price,
					Count = cart.Count
				};
				await _context.OrderDetails.AddAsync(orderDetail, cancellationToken);
				await _context.SaveChangesAsync(cancellationToken);
			}

			using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
			try
			{
                if (ecommerceUser.CompanyId.GetValueOrDefault() != 0)
				{
					return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartDTO.OrderHeader.Id});
				}
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
						SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartDTO.OrderHeader.Id}",
						CancelUrl = domain + $"customer/cart/index",
					};

				foreach (var item in ShoppingCartDTO.ListCart)
				{

					var sessionLineItem = new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							/* Convert Naira to dollars inorder to use stripe */
							UnitAmount = ((Convert.ToInt64(item.Price/446.63) * 100)/item.Count),// Convert price from cent 20.00 -> 2000
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
				//ShoppingCartDTO.OrderHeader.SessionId = session.Id;
				//ShoppingCartDTO.OrderHeader.PaymentIntentId = session.PaymentIntentId;
				UpdateStripePaymentId(ShoppingCartDTO.OrderHeader.Id, session.Id, session.PaymentIntentId);
				await _context.SaveChangesAsync(cancellationToken);
				Response.Headers.Add("Location", session.Url);
				await transaction.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(cancellationToken);
				TempData["errorMessage"] = ex.Message;
				return RedirectToAction("Index", "Home");
			}
			return new StatusCodeResult(303);
		}

		public async Task<IActionResult> OrderConfirmation(long id, CancellationToken cancellationToken)
		{
			int idint = Convert.ToUInt16(id);
			OrderHeader orderHeader = await _context.OrderHeaders.FirstOrDefaultAsync(x=> x.Id.Equals(id));
			using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
			try
			{
				if (orderHeader.PaymentStatus != Constants.PaymentStatusDelayedPayment)
				{
                    var service = new SessionService();
                    Session session = service.Get(orderHeader.SessionId);
                    /* Check the Stripe status */
                    if (session.PaymentStatus.ToLower().Equals("paid"))
                    {
						UpdateStripePaymentId(id, orderHeader.SessionId, session.PaymentIntentId);
						UpdateStatus(id, Constants.StatusApproved, Constants.PaymentStatusApproved);
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }

				IEnumerable<ShoppingCart> shoppingCarts = await _context.ShoppingCarts.Where(x => x.EcommerceUserId.Equals(
					orderHeader.EcommerceUserId))
					.ToListAsync(cancellationToken);

				/* After processing clear the shopping cart */
				_httpContextAccessor.HttpContext.Session.Clear();
				if (shoppingCarts.Any())
					_context.ShoppingCarts.RemoveRange(shoppingCarts);
				await _context.SaveChangesAsync(cancellationToken);
				await transaction.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(cancellationToken);
				TempData["errorMessage"] = ex.Message;
				return RedirectToAction("Index", "Home");
			}
			return View(idint);
		}


		public async Task<IActionResult> Plus(long cartId, CancellationToken cancellationToken)
		{
			using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
			try
			{
				var cart = await _context.ShoppingCarts.FindAsync(cartId);
				cart.Count = IncrementCount(cart, 1).Count;
				await _context.SaveChangesAsync(cancellationToken);
				await transaction.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(cancellationToken);
				TempData["errorMessage"] = ex.Message;
				return RedirectToAction("Index");
			}
			return RedirectToAction("Index");
		}
		public async Task<IActionResult> Minus(long cartId, CancellationToken cancellationToken)
		{
			using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
			try
			{
				var cart = await _context.ShoppingCarts.FindAsync(cartId);
				if (cart.Count <= 1)
				{
					_context.ShoppingCarts.Remove(cart);
                    var count = _context.ShoppingCarts.Where(x => x.EcommerceUserId.Equals(cart.EcommerceUserId))
                    .ToList().Count - 1;
                    _httpContextAccessor.HttpContext.Session.SetInt32(Constants.SessionCart, count);
                }
				cart.Count = DecrementCount(cart, 1).Count;
				await _context.SaveChangesAsync(cancellationToken);
				await transaction.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(cancellationToken);
				TempData["errorMessage"] = ex.Message;
				return RedirectToAction("Index");
			}
			return RedirectToAction("Index");
		}
		public async Task<IActionResult> Remove(long cartId, CancellationToken cancellationToken)
		{
			using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
			try
			{
				var cart = await _context.ShoppingCarts.FindAsync(cartId);
				_context.ShoppingCarts.Remove(cart);
				await _context.SaveChangesAsync(cancellationToken);
                var count =  _context.ShoppingCarts.Where(x => x.EcommerceUserId.Equals(cart.EcommerceUserId))
					.ToList().Count;
				_httpContextAccessor.HttpContext.Session.SetInt32(Constants.SessionCart, count);

                await transaction.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(cancellationToken);
				TempData["errorMessage"] = ex.Message;
				return RedirectToAction("Index");
			}
			return RedirectToAction("Index");
		}
		private static ShoppingCart DecrementCount(ShoppingCart shoppingCart, int count)
		{
			shoppingCart.Count -= count;
			return shoppingCart;
		}
		private static ShoppingCart IncrementCount(ShoppingCart shoppingCart, int count)
		{
			shoppingCart.Count += count;
			return shoppingCart;
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
	}
}
