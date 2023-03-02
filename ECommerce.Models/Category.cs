using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models
{
    //Classe Category pour utiliser comme modele
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(60)]
        public string Name { get; set; }

     
    }
}