using Ecommerce.Infrastructure.Services.Interface;
using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Services.Implementation
{
	public class CategoryRepository : ICategoryRepository
	{
		private readonly EcommerceDbContext _context;

		public CategoryRepository(EcommerceDbContext context)
		{
			_context = context;
		}
		public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
		{
			await _context.AddAsync(category, cancellationToken);
		}

		public void Delete(Category category)
		{
			_context.Remove(category);
		}

		public async Task<IEnumerable<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
		{
			return await _context.Categories.AsNoTracking().ToListAsync(cancellationToken);
		}

		public async Task<Category> GetIdAsync(long id, CancellationToken cancellationToken = default)
		{
			return await _context.Categories.FirstOrDefaultAsync(x=> x.Id.Equals(id), cancellationToken);
		}

		public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
		{
			return await _context.Categories.CountAsync(cancellationToken);
		}

		public void Update(Category category)
		{
			_context.Update(category);
		}
	}
}
