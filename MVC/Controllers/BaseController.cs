using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MVC.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? HttpContext.Session.GetString("Role");
            var name = User.Identity?.Name ?? HttpContext.Session.GetString("FullName") ?? "User";
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? HttpContext.Session.GetString("Email");

            ViewBag.UserRole = role;
            ViewBag.UserName = name;
            ViewBag.UserEmail = email;
            ViewBag.IsAuthenticated = User.Identity?.IsAuthenticated == true || !string.IsNullOrEmpty(role);

            var controllerName = context.RouteData.Values["controller"]?.ToString();
            var actionName = context.RouteData.Values["action"]?.ToString();

            if (string.Equals(controllerName, "Auth", StringComparison.OrdinalIgnoreCase) &&
                (string.Equals(actionName, "Login", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(actionName, "Register", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(actionName, "AccessDenied", StringComparison.OrdinalIgnoreCase)))
            {
                ViewBag.Layout = "_AuthLayout";
            }
            else
            {
                // Dashboard and all authenticated modules use the unified modern enterprise _Layout
                ViewBag.Layout = "_Layout";
            }

            base.OnActionExecuting(context);
        }
    }
}