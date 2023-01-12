using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Services.Interface
{
	public interface ICategoryRepository
	{
		Task<IEnumerable<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
		Task<Category> GetIdAsync(long id, CancellationToken cancellationToken = default);
		Task<int> GetCountAsync(CancellationToken cancellationToken = default);
		Task AddAsync(Category category, CancellationToken cancellationToken = default);
		void Update(Category category);
		void Delete(Category category);
		//bool Save();
	}
}
