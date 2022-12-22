using Ecommerce.Infrastructure.Data.DTO;
using Ecommerce.Infrastructure.Services.Interface;
using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceMVC.Areas.Account.Controllers
{
    [Area("Account")]
    public class RegisterController : Controller
    {
        private readonly EcommerceDbContext _context;
        private readonly UserManager<EcommerceUser> _userManager;
        private readonly SignInManager<EcommerceUser> _signInManager;

        public RegisterController(
            EcommerceDbContext context,
            UserManager<EcommerceUser> userManager,
            SignInManager<EcommerceUser> signInManager
        )
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Register(string returnUrl = null)
        {
            RegisterDTO registerDTO = new RegisterDTO();
            registerDTO.ReturnUrl = returnUrl;
            return View(registerDTO);
        }
    }
}
