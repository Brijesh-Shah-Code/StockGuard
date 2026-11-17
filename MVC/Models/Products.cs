using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC.Models
{
    public class Products
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "SKU")]
        public string SKU { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "numeric(12,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Unit Price must be greater than or equal to 0")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock Quantity must be greater than or equal to 0")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; } = 0;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Minimum Stock Level must be greater than or equal to 0")]
        [Display(Name = "Minimum Stock Level")]
        public int MinStockLevel { get; set; } = 0;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties for display (nullable - not part of form submission)
        public string? CategoryName { get; set; }
        public string? SupplierName { get; set; }
    }
}