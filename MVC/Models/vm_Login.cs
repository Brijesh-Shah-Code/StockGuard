using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MVC.Models
{
    public class vm_Login
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }
    }
}