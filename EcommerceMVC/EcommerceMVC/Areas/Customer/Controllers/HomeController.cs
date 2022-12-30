using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Data.DTO;
using Ecommerce.Infrastructure.Services.Interface;
using EcommerceMVC.Data;
using EcommerceMVC.Models;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace EcommerceMVC.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
		private readonly EcommerceDbContext _context;

		public HomeController(EcommerceDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
        {
            IEnumerable<Product> products = await _context.Products.Include(x => x.Category).AsNoTracking().ToListAsync();
			return View(products);
        }

        public async Task<IActionResult> Details(long productid, CancellationToken cancellationToken)
        {
            var product = await _context.Products.Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == productid, cancellationToken);
            ShoppingCart cart = new()
            {
                Count = 1,
                ProductId = productid,
                Product = product,
                Products = await _context.Products.Where(x => x.Category.Id.Equals(product.CategoryId) && x.Id!=productid).AsNoTracking()
                .ToListAsync(cancellationToken)
        };
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Details(ShoppingCart shoppingCart, CancellationToken cancellationToken)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                shoppingCart.EcommerceUserId = Convert.ToInt64(claim.Value);

                var cartProduct = await _context.ShoppingCarts.FirstOrDefaultAsync(x => 
                x.EcommerceUserId.Equals(Convert.ToInt64(claim.Value)) && 
                x.ProductId.Equals(shoppingCart.ProductId), cancellationToken);

                if (cartProduct == null)
                {
                    await _context.ShoppingCarts.AddAsync(shoppingCart, cancellationToken);
                }
                if (cartProduct != null)
                {
                    if (cartProduct.EcommerceUserId.Equals(Convert.ToInt64(claim.Value)))
                    {
                        cartProduct.Count = IncrementCount(cartProduct, shoppingCart.Count).Count;
                    }
                }
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Product added to cart";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                TempData["errorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            //return View(cart);
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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}