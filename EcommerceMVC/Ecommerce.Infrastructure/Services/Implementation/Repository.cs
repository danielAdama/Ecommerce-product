using Ecommerce.Infrastructure.Services.Interface;
using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Services.Implementation
{
	public class Repository<T> : IRepository<T> where T : BaseEntity
	{
		private readonly EcommerceDbContext _context;
		internal DbSet<T> dbSet;

		public Repository(EcommerceDbContext context)
		{
			_context = context;
			this.dbSet = _context.Set<T>();
		}

		public void Add(T entity)
		{
			dbSet.Add(entity);
		}

		public IEnumerable<T> GetAll()
		{
			IQueryable<T> query = dbSet;
			return query.ToList();
		}

		public T GetFirstOrDefault(Expression<Func<T, bool>> filter)
		{
			IQueryable<T> query = dbSet;
			query = query.Where(filter);
			return query.FirstOrDefault();
		}

		public void Remove(T entity)
		{
			dbSet.Remove(entity);
		}

		public void RemoveRange(IEnumerable<T> entity)
		{
			dbSet.RemoveRange(entity);
		}
	}
}
