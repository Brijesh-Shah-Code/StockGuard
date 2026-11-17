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
    [Authorize(Roles = "Admin,User")]
    public class StockTransactionsController : BaseController
    {
        private readonly ILogger<StockTransactionsController> _logger;
        private readonly StockTransactionsHelper _stockTransactionsHelper;

        public StockTransactionsController(ILogger<StockTransactionsController> logger, StockTransactionsHelper stockTransactionsHelper)
        {
            _logger = logger;
            _stockTransactionsHelper = stockTransactionsHelper;
        }

        public async Task<IActionResult> Index()
        {
            var transactions = await _stockTransactionsHelper.ReadAllStockTransactions();
            return View(transactions);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            var transaction = new StockTransactions
            {
                TransactionDate = DateTime.Now
            };
            return View(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] StockTransactions transaction)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(transaction.ProductId);
                return View(transaction);
            }

            try
            {
                var result = await _stockTransactionsHelper.CreateStockTransaction(transaction);
                if (result)
                {
                    TempData["SuccessMessage"] = "Stock transaction created successfully.";
                    return RedirectToAction("Index");
                }

                ViewBag.ErrorMessage = "Failed to create stock transaction.";
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating stock transaction");
                ModelState.AddModelError("", "An error occurred while processing the transaction: " + ex.Message);
            }

            await PopulateDropdowns(transaction.ProductId);
            return View(transaction);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int transactionId)
        {
            var transaction = await _stockTransactionsHelper.ReadStockTransactionById(transactionId);
            if (transaction == null)
            {
                return NotFound();
            }
            return View(transaction);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int transactionId)
        {
            try
            {
                var result = await _stockTransactionsHelper.DeleteStockTransaction(transactionId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Stock transaction deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete stock transaction.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting stock transaction");
                TempData["ErrorMessage"] = "An error occurred while deleting the transaction: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Display(int transactionId)
        {
            var transaction = await _stockTransactionsHelper.ReadStockTransactionById(transactionId);
            if (transaction == null)
            {
                return NotFound();
            }
            return View(transaction);
        }

        public async Task<IActionResult> ProductHistory(int productId)
        {
            var transactions = await _stockTransactionsHelper.ReadStockTransactionsByProductId(productId);
            var product = transactions.FirstOrDefault()?.ProductName ?? "Unknown Product";
            ViewBag.ProductName = product;
            ViewBag.ProductId = productId;
            return View(transactions);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStockTransactions()
        {
            var transactions = await _stockTransactionsHelper.ReadAllStockTransactions();
            return Json(transactions);
        }

        [HttpGet]
        public async Task<IActionResult> GetStockTransactionById(int transactionId)
        {
            var transaction = await _stockTransactionsHelper.ReadStockTransactionById(transactionId);
            if (transaction == null)
            {
                return NotFound();
            }
            return Json(transaction);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task PopulateDropdowns(int? selectedProductId = null)
        {
            var products = await _stockTransactionsHelper.GetProductsForDropdown();
            var productList = products.Select(p => new SelectListItem
            {
                Value = p.ProductId.ToString(),
                Text = $"{p.ProductName} ({p.SKU})"
            }).ToList();

            ViewBag.Products = new SelectList(productList, "Value", "Text", selectedProductId?.ToString());

            ViewBag.TransactionTypes = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "IN", Text = "IN - Stock In" },
                new SelectListItem { Value = "OUT", Text = "OUT - Stock Out" },
                new SelectListItem { Value = "ADJUSTMENT", Text = "ADJUSTMENT - Stock Adjustment" }
            }, "Value", "Text");
        }
    }
}