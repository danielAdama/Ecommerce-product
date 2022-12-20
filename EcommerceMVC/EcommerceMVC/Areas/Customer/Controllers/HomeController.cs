using Ecommerce.Infrastructure.Services.Interface;
using EcommerceMVC.Data;
using EcommerceMVC.Models;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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