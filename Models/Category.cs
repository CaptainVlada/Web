using System.ComponentModel.DataAnnotations;

namespace OrderAutomation.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Требуется указать название категории")]
        [Display(Name = "Название")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        public ICollection<Product>? Products { get; set; }
    }
} 