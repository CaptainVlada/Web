using System.ComponentModel.DataAnnotations;

namespace OrderAutomation.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Требуется указать название компании")]
        [Display(Name = "Название компании")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Требуется указать контактное лицо")]
        [Display(Name = "Контактное лицо")]
        public string ContactName { get; set; } = string.Empty;

        [Display(Name = "Должность")]
        public string? Position { get; set; }

        [Required(ErrorMessage = "Требуется указать номер телефона")]
        [Phone(ErrorMessage = "Неверный формат телефона")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Неверный формат email")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Адрес")]
        public string? Address { get; set; }

        [Display(Name = "ИНН")]
        public string? INN { get; set; }

        [Display(Name = "КПП")]
        public string? KPP { get; set; }

        public ICollection<Order>? Orders { get; set; }
    }
} 