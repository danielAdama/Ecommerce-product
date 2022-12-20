using Ecommerce.Infrastructure.Data.DTO;
using Ecommerce.Infrastructure.Services.Implementation;
using Ecommerce.Infrastructure.Services.Interface;
using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EcommerceMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
	[AutoValidateAntiforgeryToken]
	public class ProductController : Controller
	{
		private readonly EcommerceDbContext _context;
		private readonly ICategoryRepository _categoryRepository;

		public ProductController(EcommerceDbContext context, ICategoryRepository categoryRepository)
		{
			_context = context;
			_categoryRepository = categoryRepository;
		}

		public async Task<IActionResult> Index()
		{
			//IEnumerable<Category> productProperty = await _categoryRepository.GetAllCategoriesAsync();
			//return View(productProperty);
			return View();
		}

		public async Task<IActionResult> Upsert(long id)
		{
			ProductDTO productDTO = new()
			{
				Product = new(),
				CategoryList = (await _context.Categories.ToListAsync()).Select(x => new SelectListItem
				{
					Text = x.Name,
					Value = x.Id.ToString()
				})
			};


			if (id is not 0)
			{
				// Update product
				
				return View(productDTO);

			}
			// Create product
			return View(productDTO);
		}

		[HttpPost]
		public async Task<IActionResult> Upsert(ProductDTO obj, IFormFile file, CancellationToken cancellationToken)
		{
			var fileName = Guid.NewGuid().ToString().ToLower();
			var fileExt = file.FileName.ToString().Split(".")[1];
			var allowedExt = new string[] { "jpg", "jpeg", "png" };
			if (!allowedExt.Contains(fileExt))
			{
				TempData["success"] = "Invalid File Extension";
				RedirectToAction("Index");
			}
			var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images\productImg");
			if (!Directory.Exists(filePath))
			{
				Directory.CreateDirectory(filePath);
			}
			using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
			try
			{
				string path = $"{filePath}\\{fileName.Replace("-", "")}.{fileExt}";
				if (file.Length > 0)
				{
					obj.Product.ImageUrl = path;
					using (FileStream stream = new(path, FileMode.Create))
					{
						await file.CopyToAsync(stream, cancellationToken);
					}

					var fileStream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
				}
				await _context.Products.AddAsync(obj.Product);
				await _context.SaveChangesAsync(cancellationToken);
				await transaction.CommitAsync(cancellationToken);
				TempData["success"] = "Product created successfully";

				return View(obj);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(cancellationToken);
				TempData["errorMessage"] = ex.Message;
				return RedirectToAction("Index");
			}

		}

		//public async Task<IActionResult> Delete(long id)
		//{
		//	Category category = await _categoryRepository.GetIdAsync(id);

		//	if (category == null)
		//	{
		//		return NotFound();
		//	}
		//	return View(category);
		//}
		//[HttpPost]
		//public async Task<IActionResult> DeletePOST(long id)
		//{
		//	Category productProperty = await _categoryRepository.GetIdAsync(id);
		//	if (productProperty == null)
		//	{
		//		return NotFound();
		//	}
		//	_categoryRepository.Delete(productProperty);
		//	TempData["Success"] = "Category deleted successfully";
		//	return RedirectToAction("Index");
		//}

		#region API CALLS
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var productList = await _context.Products.Include(x => x.Category).AsNoTracking().ToListAsync();
			return Json(new { data = productList });
		}
		#endregion

	}

}

