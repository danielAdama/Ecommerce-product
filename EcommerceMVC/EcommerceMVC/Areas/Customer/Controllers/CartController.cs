using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Data.DTO;
using Ecommerce.Infrastructure.Services.Interface;
using Ecommerce.Infrastructure.Utilities;
using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace EcommerceMVC.Areas.Customer.Controllers
{
	[Area("Customer")]
    [Authorize]
	public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
#nullable disable
        public ShoppingCartDTO ShoppingCartDTO { get; set; }
		public int OrderTotal { get; set; }

		public CartController( 
			IHttpContextAccessor httpContextAccessor, 
			IUnitOfWork unitOfWork)
		{
            _httpContextAccessor = httpContextAccessor;
			_unitOfWork = unitOfWork;
        }

		public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
			/* Get all the products in a cart for a particular user */
			ShoppingCartDTO = new ShoppingCartDTO
			{
				ListCart = await _unitOfWork.Cart.GetAllUserProductsAsync(
					IdentityHelper.GetUserId(User.Identity), cancellationToken),
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
			/* Get all the products in a cart for a particular user */
			ShoppingCartDTO = new ShoppingCartDTO
			{
				ListCart = await _unitOfWork.Cart.GetAllUserProductsAsync(
                    IdentityHelper.GetUserId(User.Identity), cancellationToken),
				OrderHeader = new()
			};

			/* Retrieve all the application order details for a logged in user */
			ShoppingCartDTO.OrderHeader.EcommerceUser = await _unitOfWork.Cart.GetLoggedInUserCartAsync(
				IdentityHelper.GetUserId(User.Identity), cancellationToken);

			ShoppingCartDTO.OrderHeader.Name = ShoppingCartDTO.OrderHeader.EcommerceUser.UserName;
			ShoppingCartDTO.OrderHeader.PhoneNumber = ShoppingCartDTO.OrderHeader.EcommerceUser.PhoneNumber;
			ShoppingCartDTO.OrderHeader.StreetAddress = ShoppingCartDTO.OrderHeader.EcommerceUser.StreetAddress;
			ShoppingCartDTO.OrderHeader.City = ShoppingCartDTO.OrderHeader.EcommerceUser.City;
			ShoppingCartDTO.OrderHeader.State = ShoppingCartDTO.OrderHeader.EcommerceUser.State;
			ShoppingCartDTO.OrderHeader.PostalCode = ShoppingCartDTO.OrderHeader.EcommerceUser.PostalCode;
			ShoppingCartDTO.OrderHeader.OrderDate = DateTimeOffset.UtcNow;
            
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
			ShoppingCartDTO.ListCart = await _unitOfWork.Cart.GetAllUserProductsAsync(
				IdentityHelper.GetUserId(User.Identity), cancellationToken);

			ShoppingCartDTO.OrderHeader.OrderDate = DateTimeOffset.UtcNow;
			ShoppingCartDTO.OrderHeader.EcommerceUserId = IdentityHelper.GetUserId(User.Identity);

			foreach (var cart in ShoppingCartDTO.ListCart)
			{
				var price = (cart.Count * cart.Product.Price);
				cart.Price = price;
				ShoppingCartDTO.OrderHeader.OrderTotal += price;
			}

			/* If it's a company user allow them to make order without redirecting them to the 
			 * stripe, but company users are meant to pay between 30 days. */
			EcommerceUser ecommerceUser = await _unitOfWork.Cart.GetUserIdAsync(IdentityHelper.GetUserId(User.Identity));
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

			await _unitOfWork.Order.AddAsync(ShoppingCartDTO.OrderHeader, cancellationToken);
			await _unitOfWork.SaveAsync(cancellationToken);

			foreach (var cart in ShoppingCartDTO.ListCart)
			{
				OrderDetail orderDetail = new()
				{
					ProductId = cart.ProductId,
					OrderId = ShoppingCartDTO.OrderHeader.Id,
					Price = cart.Price,
					Count = cart.Count
				};
                await _unitOfWork.Order.AddOrderDetailAsync(orderDetail, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);
            }

			using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
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
                ShoppingCartDTO.OrderHeader.TrackingNumber = Guid.NewGuid().ToString();
				ShoppingCartDTO.OrderHeader.Carrier = "GIG";
				_unitOfWork.Order.UpdateStripePaymentId(ShoppingCartDTO.OrderHeader.Id, session.Id, session.PaymentIntentId);
				await _unitOfWork.SaveAsync(cancellationToken);
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
			OrderHeader orderHeader = await _unitOfWork.Order.GetIdAsync(id, cancellationToken);
			using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
			try
			{
				if (orderHeader.PaymentStatus != Constants.PaymentStatusDelayedPayment)
				{
                    var service = new SessionService();
                    Session session = service.Get(orderHeader.SessionId);
                    /* Check the Stripe status */
                    if (session.PaymentStatus.ToLower().Equals("paid"))
                    {
                        _unitOfWork.Order.UpdateStripePaymentId(id, orderHeader.SessionId, session.PaymentIntentId);
                        _unitOfWork.Order.UpdateStatus(id, Constants.StatusApproved, Constants.PaymentStatusApproved);
                        await _unitOfWork.SaveAsync(cancellationToken);
					}
				}

				IEnumerable<ShoppingCart> shoppingCarts = await _unitOfWork.Cart.GetAllCartUsersAsync(orderHeader.EcommerceUserId,
					cancellationToken);

				/* After processing clear the shopping cart */
				_httpContextAccessor.HttpContext.Session.Clear();
				if (shoppingCarts.Any())
					_unitOfWork.Cart.DeleteRange(shoppingCarts);
				await _unitOfWork.SaveAsync(cancellationToken);
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
			using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
			try
			{
				var cart = await _unitOfWork.Cart.GetIdAsync(cartId, cancellationToken);
				cart.Count = _unitOfWork.Cart.IncrementCount(cart, 1).Count;
				await _unitOfWork.SaveAsync(cancellationToken);
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
			using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
			try
			{
				var cart = await _unitOfWork.Cart.GetIdAsync(cartId, cancellationToken);
                if (cart.Count <= 1)
				{
                    _unitOfWork.Cart.Delete(cart);
                    var count = _unitOfWork.Cart.GetUserProductCount(cart.EcommerceUserId) - 1;
                    _httpContextAccessor.HttpContext.Session.SetInt32(Constants.SessionCart, count);
                }
				cart.Count = _unitOfWork.Cart.DecrementCount(cart, 1).Count;
				await _unitOfWork.SaveAsync(cancellationToken);
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
			using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
			try
			{
				var cart = await _unitOfWork.Cart.GetIdAsync(cartId, cancellationToken);
                _unitOfWork.Cart.Delete(cart);
				await _unitOfWork.SaveAsync(cancellationToken);
                var count =  _unitOfWork.Cart.GetUserProductCount(cart.EcommerceUserId);
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

	}
}
