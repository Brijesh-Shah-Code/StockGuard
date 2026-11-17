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
    public class SuppliersController : BaseController
    {
        private readonly ILogger<SuppliersController> _logger;
        private readonly SuppliersHelper _suppliersHelper;

        public SuppliersController(ILogger<SuppliersController> logger, SuppliersHelper suppliersHelper)
        {
            _logger = logger;
            _suppliersHelper = suppliersHelper;
        }

        public async Task<IActionResult> Index()
        {
            var suppliers = await _suppliersHelper.ReadAllSuppliers();
            return View(suppliers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Suppliers supplier)
        {
            if (!ModelState.IsValid)
            {
                return View(supplier);
            }

            var result = await _suppliersHelper.CreateSupplier(supplier);
            if (result)
            {
                TempData["SuccessMessage"] = "Supplier created successfully.";
                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "Failed to create supplier.";
            return View(supplier);
        }

        public async Task<IActionResult> Update(int supplierId)
        {
            var supplier = await _suppliersHelper.ReadSupplierById(supplierId);
            if (supplier == null)
            {
                return NotFound();
            }
            return View(supplier);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromForm] Suppliers supplier)
        {
            if (!ModelState.IsValid)
            {
                return View(supplier);
            }

            var result = await _suppliersHelper.UpdateSupplier(supplier);
            if (result)
            {
                TempData["SuccessMessage"] = "Supplier updated successfully.";
                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "Failed to update supplier.";
            return View(supplier);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int supplierId)
        {
            var supplier = await _suppliersHelper.ReadSupplierById(supplierId);
            if (supplier == null)
            {
                return NotFound();
            }
            return View(supplier);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int supplierId)
        {
            var result = await _suppliersHelper.DeleteSupplier(supplierId);
            if (result)
            {
                TempData["SuccessMessage"] = "Supplier deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete supplier.";
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Display(int supplierId)
        {
            var supplier = await _suppliersHelper.ReadSupplierById(supplierId);
            if (supplier == null)
            {
                return NotFound();
            }
            return View(supplier);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSuppliers()
        {
            var suppliers = await _suppliersHelper.ReadAllSuppliers();
            return Json(suppliers);
        }

        [HttpGet]
        public async Task<IActionResult> GetSupplierById(int supplierId)
        {
            var supplier = await _suppliersHelper.ReadSupplierById(supplierId);
            if (supplier == null)
            {
                return NotFound();
            }
            return Json(supplier);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}