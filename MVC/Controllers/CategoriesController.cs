using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVC.BAL;
using MVC.Models;

namespace MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : BaseController
    {
        private readonly ILogger<CategoriesController> _logger;
        private readonly CategoriesHelper _categoriesHelper;

        public CategoriesController(ILogger<CategoriesController> logger, CategoriesHelper categoriesHelper)
        {
            _logger = logger;
            _categoriesHelper = categoriesHelper;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoriesHelper.ReadAllCategories();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Categories category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            var result = await _categoriesHelper.CreateCategory(category);
            if (result)
            {
                TempData["SuccessMessage"] = "Category created successfully.";
                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "Failed to create category.";
            return View(category);
        }

        public async Task<IActionResult> Update(int categoryId)
        {
            var category = await _categoriesHelper.ReadCategoryById(categoryId);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromForm] Categories category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            var result = await _categoriesHelper.UpdateCategory(category);
            if (result)
            {
                TempData["SuccessMessage"] = "Category updated successfully.";
                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "Failed to update category.";
            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int categoryId)
        {
            var category = await _categoriesHelper.ReadCategoryById(categoryId);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int categoryId)
        {
            var result = await _categoriesHelper.DeleteCategory(categoryId);
            if (result)
            {
                TempData["SuccessMessage"] = "Category deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete category.";
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Display(int categoryId)
        {
            var category = await _categoriesHelper.ReadCategoryById(categoryId);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoriesHelper.ReadAllCategories();
            return Json(categories);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryById(int categoryId)
        {
            var category = await _categoriesHelper.ReadCategoryById(categoryId);
            if (category == null)
            {
                return NotFound();
            }
            return Json(category);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}