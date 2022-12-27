using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Data.DTO;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
				.ToListAsync(cancellationToken)
			};
			foreach(var cart in ShoppingCartDTO.ListCart)
			{
				var price = (cart.Count * cart.Product.Price);
				cart.Price = price;
				ShoppingCartDTO.TotalCartPrice += price;
			}
			return View(ShoppingCartDTO);
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
	}
}
