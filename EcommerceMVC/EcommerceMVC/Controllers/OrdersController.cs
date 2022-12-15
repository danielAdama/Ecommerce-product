using Microsoft.AspNetCore.Mvc;

namespace EcommerceMVC.Controllers
{
	public class OrdersController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
