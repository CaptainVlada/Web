using System.ComponentModel.DataAnnotations;

namespace OrderAutomation.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Требуется указать наименование поставщика")]
        [Display(Name = "Наименование")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Контактное лицо")]
        public string? ContactPerson { get; set; }

        [Display(Name = "Телефон")]
        public string? Phone { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        public string? Email { get; set; }

        [Display(Name = "Адрес")]
        public string? Address { get; set; }

        [Display(Name = "Комментарий")]
        public string? Notes { get; set; }

        public ICollection<ProductReceipt>? ProductReceipts { get; set; }
    }
} 