using Ecommerce.Infrastructure.Data;
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
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly EcommerceDbContext _context;
        private bool _disposed;
        public UnitOfWork(EcommerceDbContext context)
        {
            _context = context;
            Category = new CategoryRepository(_context);
            Company = new CompanyRepository(_context);
            Order = new OrderRepository(_context);
            Cart = new CartRepository(_context);
        }
        public ICategoryRepository Category { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IOrderRepository Order { get; private set; }
        public ICartRepository Cart { get; private set; }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        protected void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
