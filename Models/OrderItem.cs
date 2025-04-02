using System.ComponentModel.DataAnnotations;

namespace OrderAutomation.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Заказ")]
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        [Required(ErrorMessage = "Количество обязательно")]
        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше нуля")]
        [Display(Name = "Количество")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Цена за единицу обязательна")]
        [Display(Name = "Цена за единицу")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Сумма")]
        [DataType(DataType.Currency)]
        public decimal TotalPrice => Quantity * UnitPrice;
    }
} 