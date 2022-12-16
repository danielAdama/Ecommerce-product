using Ecommerce.Infrastructure.Services.Interface;
using EcommerceMVC.Services.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Services.Implementation
{
	public class UnitOfWork : IUnitOfWork
	{
		private EcommerceDbContext _context;
		public UnitOfWork(EcommerceDbContext context)
		{
			_context = context;
			Category = new CategoryRepository(_context);
		}

		public ICategoryRepository Category { get; private set; }

		public void Save()
		{
			_context.SaveChanges();
		}
	}
}
