using Ecommerce.Infrastructure.Services.Interface;
using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Services.Implementation
{
	public class CategoryRepository : Repository<Category>, ICategoryRepository
	{
		private EcommerceDbContext _context;
		public CategoryRepository(EcommerceDbContext context) : base(context)
		{
			_context = context;
		}

		public void Save()
		{
			_context.SaveChanges();
		}

		public void Update(Category obj)
		{
			_context.Categories.Update(obj);
		}
	}
}
