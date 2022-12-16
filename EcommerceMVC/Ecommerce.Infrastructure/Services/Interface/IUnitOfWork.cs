using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Services.Interface
{
	public interface IUnitOfWork
	{
		ICategoryRepository Category { get; }
		void Save();
	}
}
