using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Models
{
    //classe Product pour utiliser comme modele
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }
        public string? Brand { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        [Display(Name ="Categorie")]
        public int CategoryId { get; set; }

        [ValidateNever]
        public Category? Category { get; set; }

        [ValidateNever]
        public string? ImageUrl { get; set; }

    }
}
