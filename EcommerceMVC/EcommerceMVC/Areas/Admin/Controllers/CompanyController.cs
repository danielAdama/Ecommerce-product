using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Data.DTO;
using Ecommerce.Infrastructure.Services.Interface;
using Ecommerce.Infrastructure.Utilities;
using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using System.Data.Entity;

namespace EcommerceMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = Constants.RoleAdmin)]
	public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ViewResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert(long id, CancellationToken cancellationToken)
        {
            Company company = new();


			if (id != 0)
			{
                var getCompany = await _unitOfWork.Company.GetIdAsync(id, cancellationToken);
				return View(getCompany);
            }
			return View(company);

		}

        [HttpPost]
        public async Task<IActionResult> Upsert(Company company, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    if (company.Id == 0)
                    {
                        await _unitOfWork.Company.AddAsync(company, cancellationToken);
                    }
                    if (company.Id != 0)
                    {
                        _unitOfWork.Company.Update(company);
                        TempData["success"] = "Company updated successfully";
                    }
                    await _unitOfWork.SaveAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    TempData["success"] = "Company created successfully";
					return RedirectToAction("Index");
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
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var companyList = await _unitOfWork.Company.GetAllCompaniesAsync(cancellationToken);
            return Json(new { data = companyList });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            var company = await _unitOfWork.Company.GetIdAsync(id, cancellationToken);

            if (company == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            if (await _unitOfWork.Company.GetCompanyUserAsync(company))
            {
                return Json(new { success = false, message = "Cannot delete this company because it has an active user" });
            }

            _unitOfWork.Company.Delete(company);
            await _unitOfWork.SaveAsync(cancellationToken);
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion

    }
}

