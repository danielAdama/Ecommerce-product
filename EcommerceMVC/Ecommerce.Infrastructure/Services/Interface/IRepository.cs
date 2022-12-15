using EcommerceMVC.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Services.Interface
{
	public interface IRepository<T> where T : BaseEntity 
	{
		IEnumerable<T> GetAll();
		T GetFirstOrDefault(Expression<Func<T, bool>> filter);
		void Add(T entity);
		void Remove(T entity);
		void RemoveRange(IEnumerable<T> entity);
	}
}
