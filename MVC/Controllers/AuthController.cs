using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVC.Models;

namespace MVC.Controllers
{
    public class AuthController : BaseController
    {
        private readonly ILogger<AuthController> _logger;
        private readonly MVC.BAL.AuthHelper _authHelper;

        public AuthController(ILogger<AuthController> logger, MVC.BAL.AuthHelper authHelper)
        {
            _logger = logger;
            _authHelper = authHelper;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromForm] vm_Login user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            var result = await _authHelper.Login(user);
            if (result == null || result.UserId <= 0)
            {
                _logger.LogWarning("Failed login attempt for {Email}", user.Email);
                ViewBag.ErrorMessage = "Invalid email or password.";
                return View(user);
            }

            // Issue ASP.NET Core Cookie Authentication ticket
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()),
                new Claim(ClaimTypes.Name, result.FullName ?? result.Email),
                new Claim(ClaimTypes.Email, result.Email),
                new Claim(ClaimTypes.Role, result.Role ?? "User")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            HttpContext.Session.SetString("UserId", result.UserId.ToString());
            HttpContext.Session.SetString("FullName", result.FullName ?? string.Empty);
            HttpContext.Session.SetString("Email", result.Email ?? string.Empty);
            HttpContext.Session.SetString("Role", result.Role ?? string.Empty);

            TempData["SuccessMessage"] = $"Welcome back, {result.FullName}!";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] Users user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            user.IsActive = true;

            var result = await _authHelper.Register(user);
            if (result == 1)
            {
                TempData["SuccessMessage"] = "Registration successful! You can now log in with your credentials.";
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.ErrorMessage = (result == 0)
                ? "An account with this email address already exists."
                : "Registration failed. Please verify your details and try again.";
            return View(user);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            Response.Cookies.Delete(".AspNetCore.Session");
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}