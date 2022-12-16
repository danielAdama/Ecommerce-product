using Ecommerce.Infrastructure.Services.Interface;
using EcommerceMVC.Data;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
	[AutoValidateAntiforgeryToken]
	public class CategoryController : Controller
	{
		private readonly ICategoryRepository _categoryRepository;

		public CategoryController(ICategoryRepository categoryRepository)
		{
			_categoryRepository = categoryRepository;
		}

		public async Task<IActionResult> Index()
		{
			IEnumerable<Category> productProperty = await _categoryRepository.GetAllCategoriesAsync();
			return View(productProperty);
		}
		public ViewResult Create()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Create(Category productProperty)
		{
			if (productProperty.Name == productProperty.DisplayOrder.ToString())
			{
				ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name.");
			}

			if (ModelState.IsValid)
			{
				_categoryRepository.Add(productProperty);
				TempData["Success"] = "Category created successfully";
				return RedirectToAction("Index");
			}
			return View(productProperty);
		}

		public async Task<IActionResult> Edit(long id)
		{
			Category category = await _categoryRepository.GetIdAsync(id);

			if (category == null)
			{
				return NotFound();
			}
			return View(category);
		}

		[HttpPost]
		public IActionResult Edit(Category productProperty)
		{
			if (productProperty.Name == productProperty.DisplayOrder.ToString())
			{
				ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name.");
			}
			if (ModelState.IsValid)
			{
				_categoryRepository.Update(productProperty);
				TempData["Success"] = "Category updated successfully";
				return RedirectToAction("Index");
			}
			return View(productProperty);
		}

		public async Task<IActionResult> Delete(long id)
		{
			Category category = await _categoryRepository.GetIdAsync(id);

			if (category == null)
			{
				return NotFound();
			}
			return View(category);
		}
		[HttpPost]
		public async Task<IActionResult> DeletePOST(long id)
		{
			Category productProperty = await _categoryRepository.GetIdAsync(id);
			if (productProperty == null)
			{
				return NotFound();
			}
			_categoryRepository.Delete(productProperty);
			TempData["Success"] = "Category deleted successfully";
			return RedirectToAction("Index");
		}


	}

}

