using Microsoft.AspNetCore.Mvc;

namespace EcommerceMVC.Controllers
{
	public class AccountsController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
