using Ecommerce.Infrastructure.Services.Interface;
using Ecommerce.Infrastructure.Utilities;
using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = Constants.RoleAdmin)]
	public class CategoryController : Controller
	{
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
        }

		public async Task<IActionResult> Index()
		{
			IEnumerable<Category> productProperty = await _unitOfWork.Category.GetAllCategoriesAsync();
			return View(productProperty);
		}
		public ViewResult Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(Category productProperty, CancellationToken cancellationToken)
		{
			if (productProperty.Name == productProperty.DisplayOrder.ToString())
			{
				ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name.");
			}

			if (ModelState.IsValid)
			{
				using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
				try
				{
                    await _unitOfWork.Category.AddAsync(productProperty, cancellationToken);
                    await _unitOfWork.SaveAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    TempData["Success"] = "Category created successfully";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    TempData["errorMessage"] = ex.Message;
                    return RedirectToAction("Index");
                }
            }
			return View(productProperty);
		}

		public async Task<IActionResult> Edit(long id)
		{
			Category category = await _unitOfWork.Category.GetIdAsync(id);

			if (category == null)
			{
				return NotFound();
			}
			return View(category);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(Category productProperty, CancellationToken cancellationToken)
		{
			if (productProperty.Name == productProperty.DisplayOrder.ToString())
			{
				ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name.");
			}
			if (ModelState.IsValid)
			{
				_unitOfWork.Category.Update(productProperty);
				await _unitOfWork.SaveAsync(cancellationToken);
				TempData["Success"] = "Category updated successfully";
				return RedirectToAction("Index");
			}
			return View(productProperty);
		}

		public async Task<IActionResult> Delete(long id)
		{
			Category category = await _unitOfWork.Category.GetIdAsync(id);

			if (category == null)
			{
				return NotFound();
			}
			return View(category);
		}
		[HttpPost]
		public async Task<IActionResult> DeletePOST(long id, CancellationToken cancellationToken)
		{
			Category productProperty = await _unitOfWork.Category.GetIdAsync(id);
			if (productProperty.Equals(null))
			{
				return NotFound();
			}
			_unitOfWork.Category.Delete(productProperty);
			await _unitOfWork.SaveAsync(cancellationToken);
			TempData["Success"] = "Category deleted successfully";
			return RedirectToAction("Index");
		}


	}

}

