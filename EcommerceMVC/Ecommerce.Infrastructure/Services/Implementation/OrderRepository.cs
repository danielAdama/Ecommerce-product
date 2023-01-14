using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Services.Interface;
using Ecommerce.Infrastructure.Utilities;
using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;
using System.Web.Providers.Entities;

namespace Ecommerce.Infrastructure.Services.Implementation
{
	public class OrderRepository : IOrderRepository
	{
		private readonly EcommerceDbContext _context;

		public OrderRepository(EcommerceDbContext context)
		{
			_context = context;
		}
		public async Task AddAsync(OrderHeader order, CancellationToken cancellationToken = default)
		{
			await _context.AddAsync(order, cancellationToken);
		}
        public async Task AddOrderDetailAsync(OrderDetail order, CancellationToken cancellationToken = default)
        {
            await _context.AddAsync(order, cancellationToken);
        }
        public void Delete(OrderHeader order)
		{
			_context.Remove(order);
		}

		public async Task<IEnumerable<OrderHeader>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
		{
			return await _context.OrderHeaders.AsNoTracking().ToListAsync(cancellationToken);
		}
		public async Task<IEnumerable<OrderHeader>> GetAllUserOrdersAsync(CancellationToken cancellationToken = default)
		{
			return await _context.OrderHeaders.Include(u => u.EcommerceUser).ToListAsync(cancellationToken);
		}
		public async Task<IEnumerable<OrderHeader>> GetLoggedInUserOrdersAsync(long userId, CancellationToken cancellationToken = default)
		{
			return await _context.OrderHeaders.Where(x => x.EcommerceUserId.Equals(
                    userId)).Include(u => u.EcommerceUser).ToListAsync(cancellationToken);
		}
		public async Task<IEnumerable<OrderDetail>> GetAllOrderDetailsAsync(long orderid, CancellationToken cancellationToken = default)
		{
			return await _context.OrderDetails.Where(x => x.OrderId.Equals(orderid)).Include(u => u.Product)
					.ToListAsync(cancellationToken);
        }
		public async Task<OrderHeader> GetIdAsync(long id, CancellationToken cancellationToken = default)
		{
			return await _context.OrderHeaders.FirstOrDefaultAsync(x=> x.Id.Equals(id), cancellationToken);
		}
		public async Task<OrderHeader> GetUserOrdersAsync(long orderid, CancellationToken cancellationToken = default)
		{
			return await _context.OrderHeaders.Include(u => u.EcommerceUser).FirstOrDefaultAsync(
					x => x.Id.Equals(orderid), cancellationToken);
        }
        public void UpdateStripePaymentId(long id, string sessionId, string paymentIntentId)
        {
            var orders = _context.OrderHeaders.FirstOrDefault(x => x.Id.Equals(id));

            orders.PaymentDate = DateTimeOffset.UtcNow;
            orders.SessionId = sessionId;
            orders.PaymentIntentId = paymentIntentId;
        }
        public void UpdateStatus(long id, string orderStatus, string? paymentStatus = null)
        {
            var orders = _context.OrderHeaders.FirstOrDefault(x => x.Id.Equals(id));
            if (orders != null)
            {
                orders.OrderStatus = orderStatus;
                if (paymentStatus != null)
                {
                    orders.PaymentStatus = paymentStatus;
                }
            }
        }

        public void Update(OrderHeader order)
		{
			_context.Update(order);
		}
    }
}
