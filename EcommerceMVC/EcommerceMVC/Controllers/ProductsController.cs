using Microsoft.AspNetCore.Mvc;

namespace EcommerceMVC.Controllers
{
	public class ProductsController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
