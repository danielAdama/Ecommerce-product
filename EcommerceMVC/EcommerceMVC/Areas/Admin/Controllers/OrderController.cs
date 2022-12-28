using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Services.Interface;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading;

namespace EcommerceMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
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

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            /* We need to load all the orders based on the user(email) */
            IEnumerable<OrderHeader> orderHeaders;
            orderHeaders = await _context.OrderHeaders.Include(u => u.EcommerceUser).ToListAsync();
            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}
