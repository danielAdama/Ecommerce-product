using Ecommerce.Infrastructure.Data;
using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Services.Interface
{
	public interface IOrderRepository
	{
		Task<IEnumerable<OrderHeader>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
		Task<OrderHeader> GetIdAsync(long id, CancellationToken cancellationToken = default);
		Task<OrderHeader> GetUserOrdersAsync(long orderid, CancellationToken cancellationToken = default);
		Task<IEnumerable<OrderDetail>> GetAllOrderDetailsAsync(long orderid, CancellationToken cancellationToken = default);
		Task<IEnumerable<OrderHeader>> GetAllUserOrdersAsync(CancellationToken cancellationToken = default);
		Task<IEnumerable<OrderHeader>> GetLoggedInUserOrdersAsync(long userId, CancellationToken cancellationToken = default);
		void UpdateStripePaymentId(long id, string sessionId, string paymentIntentId);
        void UpdateStatus(long id, string orderStatus, string? paymentStatus = null);
        Task AddAsync(OrderHeader order, CancellationToken cancellationToken = default);
		void Update(OrderHeader order);
		void Delete(OrderHeader order);
	}
}
