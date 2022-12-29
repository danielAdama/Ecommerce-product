using Ecommerce.Infrastructure.Data.DTO;
using Ecommerce.Infrastructure.Services.Implementation;
using Ecommerce.Infrastructure.Services.Interface;
using Ecommerce.Infrastructure.Utilities;
using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EcommerceMVC.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = Constants.RoleAdmin)]
	public class ProductController : Controller
	{
		private readonly EcommerceDbContext _context;
		private readonly ICategoryRepository _categoryRepository;

		public ProductController(EcommerceDbContext context, ICategoryRepository categoryRepository)
		{
			_context = context;
			_categoryRepository = categoryRepository;
		}

		public ViewResult Index()
		{
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
				productDTO.Product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
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
			if (obj.Product.ImageUrl != null)
			{
				var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), obj.Product.ImageUrl.TrimStart('\\'));
				if (System.IO.File.Exists(oldImagePath))
				{
					System.IO.File.Delete(oldImagePath);
				}
			}
			using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
			try
			{
				string path = $"{@"wwwroot\images\productImg\"}{fileName}.{fileExt}";
				if (file.Length > 0)
				{
					obj.Product.ImageUrl = path.Replace("wwwroot","");
					using (FileStream stream = new(path, FileMode.Create))
					{
						await file.CopyToAsync(stream, cancellationToken);
					}

					var fileStream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
				}
				if (obj.Product.Id != 0)
				{
					_context.Products.Update(obj.Product);
                    TempData["success"] = "Product updated successfully";
                }
				if (obj.Product.Id == 0)
				{
					await _context.Products.AddAsync(obj.Product, cancellationToken);
				}
				await _context.SaveChangesAsync(cancellationToken);
				await transaction.CommitAsync(cancellationToken);
				TempData["success"] = "Product created successfully";
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(cancellationToken);
				TempData["errorMessage"] = ex.Message;
				return RedirectToAction("Index");
			}
			return View(obj);
		}

		#region API CALLS
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var productList = await _context.Products.Include(x => x.Category).AsNoTracking().ToListAsync();
			return Json(new { data = productList });
		}

		[HttpDelete]
		public async Task<IActionResult> Delete(long id)
		{
			var obj = await _context.Products.FirstOrDefaultAsync(x => x.Id.Equals(id));

			if (obj == null)
			{
				return Json(new { success = false, message = "Error while deleting" });
			}

			var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), obj.ImageUrl.TrimStart('\\'));

			if (System.IO.File.Exists(oldImagePath))
			{
				System.IO.File.Delete(oldImagePath);
			}

			_context.Products.Remove(obj);
			await _context.SaveChangesAsync();
			return Json(new { success = true, message = "Deleting Successful" });
		}
		#endregion

	}

}

