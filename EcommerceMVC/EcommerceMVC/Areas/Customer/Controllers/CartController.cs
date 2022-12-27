﻿using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Data.DTO;
using Ecommerce.Infrastructure.Utilities;
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
#nullable disable
		public ShoppingCartDTO ShoppingCartDTO { get; set; }
		public int OrderTotal { get; set; }

		public CartController(EcommerceDbContext context)
		{
			_context = context;
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
            
			using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
			try
			{
				ShoppingCartDTO.OrderHeader.PaymentStatus = Constants.PaymentStatusPending;
				ShoppingCartDTO.OrderHeader.OrderStatus = Constants.StatusPending;
				ShoppingCartDTO.OrderHeader.OrderDate = DateTimeOffset.UtcNow;
				ShoppingCartDTO.OrderHeader.EcommerceUserId = Convert.ToInt64(claim.Value);

				foreach (var cart in ShoppingCartDTO.ListCart)
				{
					var price = (cart.Count * cart.Product.Price);
					cart.Price = price;
					ShoppingCartDTO.OrderHeader.OrderTotal += price;
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
				Session session = service.Create(options);
				ShoppingCartDTO.OrderHeader.SessionId = session.Id;
				ShoppingCartDTO.OrderHeader.PaymentIntentId = session.PaymentIntentId;
				//UpdateStripePaymentId(ShoppingCartDTO.OrderHeader.Id, session.Id, session.PaymentIntentId);
				await _context.SaveChangesAsync(cancellationToken);
				Response.Headers.Add("Location", session.Url);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(cancellationToken);
				TempData["errorMessage"] = ex.Message;
				return RedirectToAction("Index", "Home");
			}
            //return RedirectToAction("Index", "Home");
			return new StatusCodeResult(303);
		}

		public async Task<IActionResult> OrderConfirmation(long id, CancellationToken cancellationToken)
		{
			OrderHeader orderHeader = await _context.OrderHeaders.FindAsync(id);
			var service = new SessionService();
			Session session = service.Get(orderHeader.SessionId);
			using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
			try
			{
				/* Check the Stripe status */
				if (session.PaymentStatus.ToLower().Equals("paid"))
				{
					UpdateStatus(id, Constants.StatusApproved, Constants.PaymentStatusApproved);
					await _context.SaveChangesAsync(cancellationToken);
				}

				IEnumerable<ShoppingCart> shoppingCarts = await _context.ShoppingCarts.Where(x => x.EcommerceUserId.Equals(
					orderHeader.EcommerceUserId))
					.ToListAsync(cancellationToken);

				/* After processing clear the shopping cart */
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
			return View(id);
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
				return RedirectToAction(nameof(Index));
			}
			return RedirectToAction(nameof(Index));
		}
		public async Task<IActionResult> Minus(long cartId, CancellationToken cancellationToken)
		{
			using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
			try
			{
				var cart = await _context.ShoppingCarts.FindAsync(cartId);
				if (cart.Count <= 0)
				{
					_context.ShoppingCarts.Remove(cart);
				}
				cart.Count = DecrementCount(cart, 1).Count;
				await _context.SaveChangesAsync(cancellationToken);
				await transaction.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(cancellationToken);
				TempData["errorMessage"] = ex.Message;
				return RedirectToAction(nameof(Index));
			}
			return RedirectToAction(nameof(Index));
		}
		public async Task<IActionResult> Remove(long cartId, CancellationToken cancellationToken)
		{
			using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
			try
			{
				var cart = await _context.ShoppingCarts.FindAsync(cartId);
				_context.ShoppingCarts.Remove(cart);
				await _context.SaveChangesAsync(cancellationToken);
				await transaction.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(cancellationToken);
				TempData["errorMessage"] = ex.Message;
				return RedirectToAction(nameof(Index));
			}
			return RedirectToAction(nameof(Index));
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
