namespace CoffeeShop.Web.ViewModels
{
    public class CartItem
    {
        public int ProductId { get; set; } // ID товара (кофе или десерта)
        public string ProductName { get; set; } = string.Empty; // Название товара
        public decimal Price { get; set; } // Цена за единицу
        public int Quantity { get; set; } // Количество товара в корзине
        public string ImageUrl { get; set; } = string.Empty; // Изображение товара
        public string ProductType { get; set; } = string.Empty;
        // Свойство только для чтения для общей стоимости элемента
        public decimal TotalPrice => Price * Quantity;
    }
}