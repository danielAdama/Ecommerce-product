using Ecommerce.Infrastructure.Services.Interface;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Services.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EcommerceDbContext _context;
        //private readonly IDbContextTransaction _transaction;

        public UnitOfWork(EcommerceDbContext context)
        {
            _context = context;
            //_transaction = _context.Database.BeginTransaction();
            Category = new CategoryRepository(_context);
        }
        public ICategoryRepository Category { get; private set; }

        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
