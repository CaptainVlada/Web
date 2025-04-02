using System.ComponentModel.DataAnnotations;

namespace OrderAutomation.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Требуется указать наименование продукта")]
        [Display(Name = "Наименование")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Требуется указать артикул")]
        [Display(Name = "Артикул")]
        public string SKU { get; set; } = string.Empty;

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Требуется указать цену")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше нуля")]
        [Display(Name = "Цена")]
        [DataType(DataType.Text)]
        public decimal Price { get; set; }

        [Display(Name = "Количество на складе")]
        [Range(0, int.MaxValue, ErrorMessage = "Количество не может быть отрицательным")]
        public int StockQuantity { get; set; }

        [Display(Name = "Категория")]
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<OrderItem>? OrderItems { get; set; }
        public ICollection<ProductReceipt>? ProductReceipts { get; set; }
    }
} 