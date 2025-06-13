using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Для атрибута [ForeignKey]

namespace CoffeeShop.Web.Models
{
    public class Coffee
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название обязательно.")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов.")]
        public string? Description { get; set; } // "?" делает поле обнуляемым

        [Required(ErrorMessage = "Цена обязательна.")]
        [Range(0.01, 10000.00, ErrorMessage = "Цена должна быть от 0.01 до 10000.")] // Диапазон допустимых значений
        public decimal Price { get; set; }

        [Display(Name = "Ссылка на изображение")]
        public string? ImageUrl { get; set; }

        // Внешний ключ для связи с Category
        [Display(Name = "Категория")]
        [ForeignKey("Category")] // Указывает, что это внешний ключ к таблице Category
        public int CategoryId { get; set; }

        // Навигационное свойство: один напиток принадлежит одной категории
        public Category? Category { get; set; }
    }
}
