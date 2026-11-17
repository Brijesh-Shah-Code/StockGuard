using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using MVC.BAL;
using MVC.Models;

namespace MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : BaseController
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly ProductsHelper _productsHelper;

        public ProductsController(ILogger<ProductsController> logger, ProductsHelper productsHelper)
        {
            _logger = logger;
            _productsHelper = productsHelper;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productsHelper.ReadAllProducts();
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Products product)
        {
            if (await _productsHelper.ReadProductBySKU(product.SKU) != null)
            {
                ModelState.AddModelError("SKU", "SKU already exists. Please enter a unique SKU.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(product.CategoryId, product.SupplierId);
                return View(product);
            }

            try
            {
                var result = await _productsHelper.CreateProduct(product);
                if (result)
                {
                    TempData["SuccessMessage"] = "Product created successfully.";
                    return RedirectToAction("Index");
                }

                ViewBag.ErrorMessage = "Failed to create product. SKU might already exist.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                ViewBag.ErrorMessage = "An error occurred while creating the product: " + ex.Message;
            }

            await PopulateDropdowns(product.CategoryId, product.SupplierId);
            return View(product);
        }

        public async Task<IActionResult> Update(int productId)
        {
            var product = await _productsHelper.ReadProductById(productId);
            if (product == null)
            {
                return NotFound();
            }
            await PopulateDropdowns(product.CategoryId, product.SupplierId);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromForm] Products product)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(product.CategoryId, product.SupplierId);
                return View(product);
            }

            var existingProduct = await _productsHelper.ReadProductBySKU(product.SKU);
            if (existingProduct != null && existingProduct.ProductId != product.ProductId)
            {
                ModelState.AddModelError("SKU", "SKU already exists. Please enter a unique SKU.");
                await PopulateDropdowns(product.CategoryId, product.SupplierId);
                return View(product);
            }

            try
            {
                var result = await _productsHelper.UpdateProduct(product);
                if (result)
                {
                    TempData["SuccessMessage"] = "Product updated successfully.";
                    return RedirectToAction("Index");
                }

                ViewBag.ErrorMessage = "Failed to update product.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                ViewBag.ErrorMessage = "An error occurred while updating the product: " + ex.Message;
            }

            await PopulateDropdowns(product.CategoryId, product.SupplierId);
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int productId)
        {
            var product = await _productsHelper.ReadProductById(productId);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int productId)
        {
            try
            {
                var result = await _productsHelper.DeleteProduct(productId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Product deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete product. It may have associated stock transactions.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                TempData["ErrorMessage"] = "An error occurred while deleting the product: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Display(int productId)
        {
            var product = await _productsHelper.ReadProductById(productId);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productsHelper.ReadAllProducts();
            return Json(products);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductById(int productId)
        {
            var product = await _productsHelper.ReadProductById(productId);
            if (product == null)
            {
                return NotFound();
            }
            return Json(product);
        }

        [HttpGet]
        public async Task<IActionResult> CheckSKUExists(string sku, int? productId = null)
        {
            var product = await _productsHelper.ReadProductBySKU(sku);
            bool exists = product != null && (!productId.HasValue || product.ProductId != productId.Value);
            return Json(new { exists });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task PopulateDropdowns(int? selectedCategoryId = null, int? selectedSupplierId = null)
        {
            var categories = await _productsHelper.GetCategoriesForDropdown();
            var suppliers = await _productsHelper.GetSuppliersForDropdown();

            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", selectedCategoryId);
            ViewBag.Suppliers = new SelectList(suppliers, "SupplierId", "SupplierName", selectedSupplierId);
        }
    }
}