using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC.Models
{
    public class StockTransactions
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Transaction Type")]
        public string TransactionType { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Transaction Date")]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [StringLength(300)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        // Navigation properties for display (nullable )
        public string? ProductName { get; set; }
        public string? ProductSKU { get; set; }
    }
}