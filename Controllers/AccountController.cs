using DummyWebApp.Application.Common.Interfaces;
using DummyWebApp.Application.Utility;
using DummyWebApp.Domain.Entities;
using DummyWebApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace DummyWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        //GET
        public IActionResult Login(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            LoginVM loginVM = new LoginVM()
            {
                RedirectUrl = returnUrl,
            };
            return View(loginVM);
        }
        //POST 
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(loginVM.Email,
                    loginVM.Password, loginVM.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {

                    if (string.IsNullOrEmpty(loginVM.RedirectUrl))
                    {
                        return RedirectToAction(nameof(Index), "Home");
                    }
                    else
                    {
                        return LocalRedirect(loginVM.RedirectUrl);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt");
                }
            }
            return View(loginVM);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> Logout(LoginVM loginVM)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        //GET
        public IActionResult Register(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).Wait();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).Wait();
            }
            RegisterVM registerVM = new()
            {
                RoleList = _roleManager.Roles.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name
                }),
                RedirectUrl = returnUrl
            };
            return View(registerVM);
        }

        //POST 
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser()
                {
                    Name = registerVM.Name,
                    Email = registerVM.Email,
                    NormalizedEmail = registerVM.Email.ToUpper(),
                    EmailConfirmed = true,
                    UserName = registerVM.Email,
                    PhoneNumber = registerVM.PhoneNumber,
                    CreatedAt = DateTime.Now
                };
                var result = await _userManager.CreateAsync(user, registerVM.Password);
                if (result.Succeeded)
                {
                    string role = SD.Role_Customer;
                    if (!string.IsNullOrEmpty(registerVM.Role))
                    {
                        role = registerVM.Role;
                    }
                    await _userManager.AddToRoleAsync(user, role);

                    if (string.IsNullOrEmpty(registerVM.RedirectUrl))
                    {
                        return RedirectToAction(nameof(Index), "Home");
                    }
                    else
                    {
                        return LocalRedirect(registerVM.RedirectUrl);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            registerVM = new()
            {
                RoleList = _roleManager.Roles.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Name
                })
            };

            return View(registerVM);
        }
    }
}
