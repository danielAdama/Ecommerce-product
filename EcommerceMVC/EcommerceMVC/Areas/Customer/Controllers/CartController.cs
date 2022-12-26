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
				cart.Price = (cart.Count * cart.Product.Price);
			}
			return View(ShoppingCartDTO);
        }
    }
}
