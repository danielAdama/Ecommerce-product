using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Services.Interface;
using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Services.Implementation
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly EcommerceDbContext _context;
        private readonly IDbContextTransaction _transaction;

        public CompanyRepository(EcommerceDbContext context)
        {
            _context = context;
            _transaction = _context.Database.BeginTransaction();
        }

        public void AddAsync(Company company, CancellationToken cancellationToken)
        {
            _context.AddAsync(company, cancellationToken);
        }

        public void Delete(Company company)
        {
            _context.Remove(company);
        }

        public void DeleteRange(Company company)
        {
            _context.RemoveRange(company);
        }

        public Task<IEnumerable<Company>> GetAllCompaniesAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Company> GetIdAsync(long id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void SaveAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        //public async Task<IEnumerable<Company>> GetAllCompaniesAsync(CancellationToken cancellationToken)
        //{
        //    return await _context.Companies.AsNoTracking().ToListAsync(cancellationToken);
        //}

        //public async Task<Company> GetIdAsync(long id, CancellationToken cancellationToken)
        //{
        //    return await _context.Companies.FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
        //}
        //public void SaveAsync(CancellationToken cancellationToken)
        //{
        //    await _context.SaveChangesAsync(cancellationToken);
        //}

        public void Update(Company company)
        {
            _context.Update(company);
        }
    }
}
