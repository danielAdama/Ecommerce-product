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
		public bool Add(Category category)
		{
			_context.Add(category);
			return Save();
		}

		public bool Delete(Category category)
		{
			_context.Remove(category);
			return Save();
		}

		public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
		{
			return await _context.Categories.AsNoTracking().ToListAsync();
		}

		public async Task<Category> GetIdAsync(long id)
		{
			return await _context.Categories.FindAsync(id);
		}

		public async Task<int> GetCountAsync()
		{
			return await _context.Categories.CountAsync();
		}

		public bool Save()
		{
			var saved = _context.SaveChanges();
			return saved > 0;
		}

		public bool Update(Category category)
		{
			_context.Update(category);
			return Save();
		}
	}
}
