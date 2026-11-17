using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MVC.BAL;
using MVC.Models;

namespace MVC.Controllers;

public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;
    private readonly CategoriesHelper _categoriesHelper;
    private readonly SuppliersHelper _suppliersHelper;
    private readonly ProductsHelper _productsHelper;
    private readonly StockTransactionsHelper _stockTransactionsHelper;

    public HomeController(
        ILogger<HomeController> logger,
        CategoriesHelper categoriesHelper,
        SuppliersHelper suppliersHelper,
        ProductsHelper productsHelper,
        StockTransactionsHelper stockTransactionsHelper)
    {
        _logger = logger;
        _categoriesHelper = categoriesHelper;
        _suppliersHelper = suppliersHelper;
        _productsHelper = productsHelper;
        _stockTransactionsHelper = stockTransactionsHelper;
    }

    public async Task<IActionResult> Index()
    {
        // Get user info from session
        ViewBag.UserName = HttpContext.Session.GetString("FullName");
        ViewBag.UserRole = HttpContext.Session.GetString("Role");

        // Fetch counts for dashboard cards
        var categories = await _categoriesHelper.ReadAllCategories();
        var suppliers = await _suppliersHelper.ReadAllSuppliers();
        var products = await _productsHelper.ReadAllProducts();
        var recentTransactions = (await _stockTransactionsHelper.ReadAllStockTransactions())
            .Take(5)
            .ToList();

        ViewBag.CategoryCount = categories.Count;
        ViewBag.SupplierCount = suppliers.Count;
        ViewBag.ProductCount = products.Count;
        ViewBag.LowStockCount = products.Count(p => p.StockQuantity <= p.MinStockLevel && p.IsActive);
        ViewBag.RecentTransactions = recentTransactions;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}