using Ecommerce.Infrastructure.Data.DTO;
using Ecommerce.Infrastructure.Services.Interface;
using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceMVC.Areas.Account.Controllers
{
    [Area("Account")]
    public class IdentityController : Controller
    {
        private readonly EcommerceDbContext _context;
        private readonly UserManager<EcommerceUser> _userManager;
        private readonly SignInManager<EcommerceUser> _signInManager;

        public IdentityController(
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

        public async Task<IActionResult> Register()
        {
            //RegisterDTO registerDTO = new RegisterDTO();
            //registerDTO.ReturnUrl = returnUrl;
            //return View(registerDTO);
            var respose = new RegisterDTO();
            return View(respose);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO registerDTO, string? returnUrl = null)
        {
            //registerDTO.ReturnUrl = returnUrl;
            //returnUrl = returnUrl ?? Url.Content("~/");
            if (!ModelState.IsValid) return View(registerDTO);

            string email = registerDTO.EmailAddress.Trim().ToLower();
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                TempData["success"] = "This email address is already in use";
                return View(registerDTO);
            }
            var newUser = new EcommerceUser
            {
                FirstName = registerDTO.FirstName,
                LastName = registerDTO.LastName,
                Email = email,
                UserName = email,
                TimeCreated = DateTime.UtcNow,
                TimeUpdated = DateTime.UtcNow
            };

            var newUserResponse = await _userManager.CreateAsync(newUser, registerDTO.Password);
            if (newUserResponse.Succeeded)
            {
                await _signInManager.SignInAsync(newUser, isPersistent: false);
                //return LocalRedirect(returnUrl);
                return View("Home");
            }
            ModelState.AddModelError("Password", "User could not be created. Password is not unique enough");
            return View(registerDTO);

        }
    }
}
