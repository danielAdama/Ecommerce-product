using Ecommerce.Infrastructure.Data;
using EcommerceMVC.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Services.Interface
{
    public interface ICompanyRepository
    {
        Task<IEnumerable<Company>> GetAllCompaniesAsync(CancellationToken cancellationToken);
        Task<Company> GetIdAsync(long id, CancellationToken cancellationToken);
        void AddAsync(Company company, CancellationToken cancellationToken);
        void Update(Company company);
        void Delete(Company company);
        void DeleteRange(Company company);
        void SaveAsync(CancellationToken cancellationToken);
    }
}
