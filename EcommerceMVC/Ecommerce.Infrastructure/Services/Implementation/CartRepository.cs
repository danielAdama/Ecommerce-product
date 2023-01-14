using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Services.Interface;
using Ecommerce.Infrastructure.Utilities;
using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;
using System.Web.Helpers;
using System.Web.Providers.Entities;

namespace Ecommerce.Infrastructure.Services.Implementation
{
	public class CartRepository : ICartRepository
	{
		private readonly EcommerceDbContext _context;

		public CartRepository(EcommerceDbContext context)
		{
			_context = context;
		}
		public async Task AddAsync(ShoppingCart cart, CancellationToken cancellationToken = default)
		{
			await _context.AddAsync(cart, cancellationToken);
		}

		public void Delete(ShoppingCart cart)
		{
			_context.Remove(cart);
		}
        public  void DeleteRange(IEnumerable<ShoppingCart> cart)
        {
            _context.RemoveRange(cart);
        }
        public async Task<IEnumerable<ShoppingCart>> GetAllCartUsersAsync(long userId, CancellationToken cancellationToken = default)
        {
            return await _context.ShoppingCarts.Where(x => x.EcommerceUserId.Equals(userId))
                    .ToListAsync(cancellationToken);
        }
        public async Task<EcommerceUser> GetLoggedInUserCartAsync(long userId, CancellationToken cancellationToken = default)
		{
			return await _context.EcommerceUsers.FirstOrDefaultAsync(x => x.Id.Equals(userId),
				cancellationToken);
		}
        public async Task<IEnumerable<ShoppingCart>> GetAllUserProductsAsync(long userId, CancellationToken cancellationToken = default)
        {
            return await _context.ShoppingCarts.Where(x => x.EcommerceUserId.Equals(userId))
                .Include(u => u.Product)
                .ToListAsync(cancellationToken);
        }
        public async Task<EcommerceUser> GetUserIdAsync(long userId)
        {
            return await _context.EcommerceUsers.FindAsync(userId);
        }
        public ShoppingCart DecrementCount(ShoppingCart shoppingCart, int count)
        {
            shoppingCart.Count -= count;
            return shoppingCart;
        }
        public ShoppingCart IncrementCount(ShoppingCart shoppingCart, int count)
        {
            shoppingCart.Count += count;
            return shoppingCart;
        }
        public async Task<ShoppingCart> GetIdAsync(long id, CancellationToken cancellationToken = default)
        {
            return await _context.ShoppingCarts.FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
        }
        public int GetUserProductCount(long userId)
        {
            return _context.ShoppingCarts.Where(x => x.EcommerceUserId.Equals(userId))
                    .ToList().Count;
        }
        public void Update(ShoppingCart cart)
		{
			_context.Update(cart);
		}
    }
}
