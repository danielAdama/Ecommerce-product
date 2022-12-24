using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Data.DTO;
using Ecommerce.Infrastructure.Services.Interface;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly EcommerceDbContext _context;

        public CompanyController(EcommerceDbContext context)
        {
            _context = context;
        }

        public ViewResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert(long id)
        {
            Company company = new();


            if (id is not 0)
            {
                return View(company);

            }
            var getCompany = await _context.Companies.FirstOrDefaultAsync(x => x.Id == id);
            return View(getCompany);
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(Company company, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    if (company.Id == 0)
                    {
                        await _context.Companies.AddAsync(company, cancellationToken);
                    }
                    if (company.Id != 0)
                    {
                        _context.Companies.Update(company);
                        TempData["success"] = "Company updated successfully";
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    TempData["success"] = "Company created successfully";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    TempData["errorMessage"] = ex.Message;
                    return RedirectToAction("Index");
                }

            }
            return View(company);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companyList = await _context.Companies.AsNoTracking().ToListAsync();
            return Json(new { data = companyList });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(long id)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(x => x.Id == id);

            if (company == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Deleting Successful" });
        }
        #endregion

    }
}

