using System.ComponentModel.DataAnnotations;

namespace CoffeeShop.Web.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; } // Внешний ключ к заказу
        public Order? Order { get; set; } // Навигационное свойство к заказу

        [Required(ErrorMessage = "Название продукта обязательно.")]
        [StringLength(100, ErrorMessage = "Название продукта не может превышать 100 символов.")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Тип продукта обязателен.")]
        [StringLength(50, ErrorMessage = "Тип продукта не может превышать 50 символов.")]
        public string ProductType { get; set; } = string.Empty; // Например, "Coffee" или "Dessert"

        public int ProductId { get; set; } // ID товара (для связывания с Coffee/Dessert, если нужно)

        [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше нуля.")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше нуля.")]
        public int Quantity { get; set; }

        // Свойство только для чтения для общей стоимости элемента
        public decimal TotalPrice => Price * Quantity;
    }
}