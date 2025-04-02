using System.ComponentModel.DataAnnotations;

namespace OrderAutomation.Models
{
    public class ProductReceipt
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Необходимо выбрать продукт")]
        [Display(Name = "Продукт")]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required(ErrorMessage = "Дата поступления обязательна")]
        [Display(Name = "Дата поступления")]
        [DataType(DataType.Date)]
        public DateTime ReceiptDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Количество обязательно")]
        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше нуля")]
        [Display(Name = "Количество")]
        public int Quantity { get; set; }

        [Display(Name = "Поставщик")]
        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        [Display(Name = "Номер накладной")]
        public string? InvoiceNumber { get; set; }

        [Display(Name = "Закупочная цена")]
        [DataType(DataType.Currency)]
        public decimal? PurchasePrice { get; set; }

        [Display(Name = "Примечания")]
        public string? Notes { get; set; }
    }
} 