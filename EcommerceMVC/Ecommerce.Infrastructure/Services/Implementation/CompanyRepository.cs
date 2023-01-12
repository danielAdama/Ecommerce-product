using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Services.Interface;
using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Ecommerce.Infrastructure.Services.Implementation
{
	public class CompanyRepository : ICompanyRepository
	{
		private readonly EcommerceDbContext _context;

		public CompanyRepository(EcommerceDbContext context)
		{
			_context = context;
		}
		public async Task AddAsync(Company company, CancellationToken cancellationToken = default)
		{
			await _context.AddAsync(company, cancellationToken);
		}

		public void Delete(Company company)
		{
			_context.Remove(company);
		}

		public async Task<IEnumerable<Company>> GetAllCompaniesAsync(CancellationToken cancellationToken = default)
		{
			return await _context.Companies.AsNoTracking().ToListAsync(cancellationToken);
		}

		public async Task<Company> GetIdAsync(long id, CancellationToken cancellationToken = default)
		{
			return await _context.Companies.FirstOrDefaultAsync(x=> x.Id.Equals(id), cancellationToken);
		}

		public void Update(Company company)
		{
			_context.Update(company);
		}

        public async Task<bool> GetCompanyUserAsync(Company company, CancellationToken cancellationToken = default)
        {
            return await _context.EcommerceUsers.AnyAsync(x => x.CompanyId.Equals(company.Id), cancellationToken);
        }
    }
}
