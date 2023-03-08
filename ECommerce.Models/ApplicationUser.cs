using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ECommerce.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "Nom")]
        public string Name { get; set; }

        [Display(Name = "Adresse")]
        public string? StreetAddress { get; set; }

        [Display(Name = "Ville")]
        public string? City { get; set; }

        [Display(Name = "Province")]
        public string? State { get; set; }

        [Display(Name = "Code Postal")]
        public string? PostalCode { get; set; }
      
    }
}
