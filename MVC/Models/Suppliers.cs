using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MVC.Models
{
    public class Suppliers
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SupplierId { get; set; }

        [Required]
        [Display(Name="Supplier Name")]
        public string SupplierName { get; set; }

        [Required]
        [Display(Name="Contact Person")]
        public string ContactPerson { get; set; }

        [EmailAddress]
        [Display(Name ="Email")]
        public string Email { get; set; }

        [Phone]
        public string Phone { get; set; }

        public string? Address { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}