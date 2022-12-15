using Microsoft.AspNetCore.Mvc;

namespace EcommerceMVC.Controllers
{
	public class CategoryController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
