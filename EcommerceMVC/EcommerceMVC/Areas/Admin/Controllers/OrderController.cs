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
        public async Task<IActionResult> UpdateOrderDetail(long orderid, OrderDTO orderDTO)
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
                    orderHeaders = orderHeaders.Where(x=> x.PaymentStatus == Constants.PaymentStatusDelayedPayment);
                    break;

                case "inprocess":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus  == Constants.StatusInProcess); ;
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
