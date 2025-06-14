using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity; // Для связи с IdentityUser

namespace CoffeeShop.Web.Models
{
    public class Order
    {
        public int Id { get; set; }

        // Пользователь, который сделал заказ
        public string? UserId { get; set; } // ID пользователя из Identity
        public IdentityUser? User { get; set; } // Навигационное свойство к пользователю

        [Required(ErrorMessage = "Поле 'Имя' обязательно.")]
        [StringLength(100, ErrorMessage = "Имя не может превышать 100 символов.")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Поле 'Email' обязательно.")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email.")]
        public string CustomerEmail { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Поле 'Телефон' обязательно.")]
        //[Phone(ErrorMessage = "Некорректный формат телефона.")]
        //public string CustomerPhone { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Поле 'Адрес доставки' обязательно.")]
        //[StringLength(250, ErrorMessage = "Адрес доставки не может превышать 250 символов.")]
        //public string DeliveryAddress { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow; // Дата и время заказа (UTC)

       // [Range(0.01, double.MaxValue, ErrorMessage = "Общая сумма заказа должна быть больше нуля.")]
        public decimal TotalAmount { get; set; }

        // Статус заказа (например, "Pending", "Processing", "Completed", "Cancelled")
        public string Status { get; set; } = "Pending";

        // Коллекция элементов заказа
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}