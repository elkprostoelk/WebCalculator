using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebCalculator.App.Models;

namespace WebCalculator.App.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(WelcomeModel welcomeModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = _dbContext.Users.FirstOrDefault(u => u.UserName == welcomeModel.UserName);

            if (user == null)
            {
                return NotFound($"User {welcomeModel.UserName} is not found!");
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, welcomeModel.Password, welcomeModel.RememberMe, lockoutOnFailure: false);
            if (signInResult == Microsoft.AspNetCore.Identity.SignInResult.Success)
            {
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "Ошибка авторизации");
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel registerModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            try
            {
                var user = new IdentityUser() { UserName = registerModel.UserName };
                var result = await _userManager.CreateAsync(user, registerModel.Password);
                if (result.Succeeded)
                {
                    await _dbContext.SaveChangesAsync();
                    return RedirectToAction("Index", "Home");
                }
                return RedirectToAction("Error", "Home", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", "Home", new ErrorViewModel { 
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = e.Message
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
