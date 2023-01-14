using Ecommerce.Infrastructure.Data;
using EcommerceMVC.Data;

namespace Ecommerce.Infrastructure.Services.Interface
{
	public interface ICartRepository
	{
		int GetUserProductCount(long userId);
        Task<EcommerceUser> GetUserIdAsync(long userId);
		Task<ShoppingCart> GetIdAsync(long cartId, CancellationToken cancellationToken = default);
        ShoppingCart DecrementCount(ShoppingCart shoppingCart, int count);
		ShoppingCart IncrementCount(ShoppingCart shoppingCart, int count);
		Task<IEnumerable<ShoppingCart>> GetAllCartUsersAsync(long userId, CancellationToken cancellationToken = default);
		Task<EcommerceUser> GetLoggedInUserCartAsync(long userId, CancellationToken cancellationToken = default);
		Task<IEnumerable<ShoppingCart>> GetAllUserProductsAsync(long userId, CancellationToken cancellationToken = default);
        Task AddAsync(ShoppingCart cart, CancellationToken cancellationToken = default);
		void Update(ShoppingCart cart);
		void Delete(ShoppingCart cart);
        void DeleteRange(IEnumerable<ShoppingCart> cart);
    }
}
