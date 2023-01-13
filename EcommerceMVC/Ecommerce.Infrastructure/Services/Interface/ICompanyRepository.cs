using Ecommerce.Infrastructure.Data;
using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Services.Interface
{
	public interface ICompanyRepository
	{
		Task<IEnumerable<Company>> GetAllCompaniesAsync(CancellationToken cancellationToken = default);
		Task<Company> GetIdAsync(long id, CancellationToken cancellationToken = default);
		Task<bool> GetCompanyUserAsync(Company company, CancellationToken cancellationToken = default);
		Task AddAsync(Company company, CancellationToken cancellationToken = default);
		void Update(Company company);
		void Delete(Company company);
	}
}
