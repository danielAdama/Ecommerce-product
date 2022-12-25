using Ecommerce.Infrastructure.Data.DTO;
using Ecommerce.Infrastructure.Services.Interface;
using Ecommerce.Infrastructure.Utilities;
using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading;
using System.Web.Helpers;

namespace EcommerceMVC.Areas.Account.Controllers
{
    [Area("Account")]
    public class IdentityController : Controller
    {
        private readonly EcommerceDbContext _context;
        private readonly UserManager<EcommerceUser> _userManager;
        private readonly SignInManager<EcommerceUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public IdentityController(
            EcommerceDbContext context,
            UserManager<EcommerceUser> userManager,
            SignInManager<EcommerceUser> signInManager,
            RoleManager<ApplicationRole> roleManager
        )
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login(string? returnUrl = null)
        {
            LoginDTO loginDTO = new LoginDTO();
            loginDTO.ReturnUrl = returnUrl ?? Url.Content("~/");
            return View(loginDTO);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO loginDTO, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                string email = loginDTO.EmailAddress.Trim().ToLower();
                var user = await _userManager.FindByEmailAsync(email);

                if (user != null)
                {
                    var passwordCheck = await _userManager.CheckPasswordAsync(user, loginDTO.Password);
                    if (!passwordCheck)
                    {
                        TempData["errorMessage"] = "Wrong password. Please try again";
                        return View(loginDTO);
                    }
                    var result = await _signInManager.PasswordSignInAsync(user, loginDTO.Password, loginDTO.RememberMe, lockoutOnFailure: false);
                    if (!result.Succeeded)
                    {
                        TempData["errorMessage"] = "Invalid Login attempt. Please try again";
                        return View(loginDTO);
                    }
                    TempData["success"] = "Log In successful";
                    return RedirectToAction("Index", "Home", new { area = "Customer"});
                }
            }
            return View(loginDTO);
        }

        [HttpGet]
        public async Task<IActionResult> Register(string? returnUrl = null)
        {
            if (!await _roleManager.RoleExistsAsync(Constants.RoleAdmin))
            {
                await _roleManager.CreateAsync(new ApplicationRole
                {
                    Name = Constants.RoleAdmin,
                });
                await _roleManager.CreateAsync(new ApplicationRole
                {
                    Name = Constants.RoleEmployee,
                });
                await _roleManager.CreateAsync(new ApplicationRole
                {
                    Name = Constants.RoleUserIndividual,
                });
                await _roleManager.CreateAsync(new ApplicationRole
                {
                    Name = Constants.RoleUserCompany,
                });
            }
            RegisterDTO registerDTO = new RegisterDTO()
            {
                RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i
                })
            };
            registerDTO.ReturnUrl = returnUrl;
            return View(registerDTO);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO registerDTO, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            registerDTO.ReturnUrl = returnUrl;
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                string email = registerDTO.EmailAddress.Trim().ToLower();
                var user = await _userManager.FindByEmailAsync(email);

                if (user != null)
                {
                    TempData["success"] = "This email address is already in use";
                    return View(registerDTO);
                }
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var newUser = new EcommerceUser
                    {
                        FirstName = registerDTO.FirstName,
                        LastName = registerDTO.LastName,
                        Email = email,
                        UserName = email,
                        StreetAddress = registerDTO.StreetAddress,
                        City = registerDTO.City,
                        State = registerDTO.State,
                        PostalCode = registerDTO.PostalCode,
                        PhoneNumber = registerDTO.PhoneNumber,
                        TimeCreated = DateTime.UtcNow,
                        TimeUpdated = DateTime.UtcNow
                    };

                    var newUserResponse = await _userManager.CreateAsync(newUser, registerDTO.Password);
                    if (!newUserResponse.Succeeded)
                    {
                        //ModelState.AddModelError("Password", "User could not be created. Password is not unique enough");
                        TempData["errorMessage"] = "User could not be created. Password is not unique enough";
                    }
                    if (registerDTO.Role == null)
                    {
                        await _userManager.AddToRoleAsync(newUser, Constants.RoleUserIndividual);
                    }
                    await _userManager.AddToRoleAsync(newUser, registerDTO.Role);
                    await _signInManager.SignInAsync(newUser, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    TempData["errorMessage"] = ex.Message;
                    return RedirectToAction("Register");
                }
            }
            return View(registerDTO);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index","Home");
        }
    }
}
