using System.ComponentModel.DataAnnotations;

namespace OrderAutomation.Models
{
    public enum OrderStatus
    {
        [Display(Name = "Новый")]
        New,
        [Display(Name = "В обработке")]
        Processing,
        [Display(Name = "Выполнен")]
        Completed,
        [Display(Name = "Отменен")]
        Canceled
    }

    public class Order
    {
        public int Id { get; set; }

        [Display(Name = "Номер заказа")]
        public string OrderNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Дата создания обязательна")]
        [Display(Name = "Дата создания")]
        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Статус")]
        public OrderStatus Status { get; set; } = OrderStatus.New;

        [Display(Name = "Дата выполнения")]
        [DataType(DataType.Date)]
        public DateTime? CompletedDate { get; set; }

        [Display(Name = "Примечание")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Необходимо выбрать клиента")]
        [Display(Name = "Клиент")]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Display(Name = "Общая сумма")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem>? OrderItems { get; set; }
    }
} 