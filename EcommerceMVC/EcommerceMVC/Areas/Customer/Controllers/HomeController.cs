using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Data.DTO;
using Ecommerce.Infrastructure.Services.Interface;
using Ecommerce.Infrastructure.Utilities;
using EcommerceMVC.Data;
using EcommerceMVC.Models;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;

namespace EcommerceMVC.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
		private readonly EcommerceDbContext _context;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public HomeController(EcommerceDbContext context, IHttpContextAccessor httpContextAccessor)
		{
			_context = context;
            _httpContextAccessor = httpContextAccessor;
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
                //var claimsIdentity = (ClaimsIdentity)User.Identity;
                //var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                shoppingCart.EcommerceUserId = IdentityHelper.GetUserId(User.Identity);

                var cartProduct = await _context.ShoppingCarts.FirstOrDefaultAsync(x => 
                x.EcommerceUserId.Equals(IdentityHelper.GetUserId(User.Identity)) && 
                x.ProductId.Equals(shoppingCart.ProductId), cancellationToken);

                if (cartProduct == null)
                {
                    await _context.ShoppingCarts.AddAsync(shoppingCart, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    _httpContextAccessor.HttpContext.Session.SetInt32(Constants.SessionCart, _context.ShoppingCarts.Where(
                        x => x.EcommerceUserId.Equals(IdentityHelper.GetUserId(User.Identity)))
                        .ToList().Count);
                }
                if (cartProduct != null)
                {
                    if (cartProduct.EcommerceUserId.Equals(IdentityHelper.GetUserId(User.Identity)))
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
                return RedirectToAction("Index");
            }
            //return View(cart);
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