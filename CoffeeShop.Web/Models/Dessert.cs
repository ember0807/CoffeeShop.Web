using System.ComponentModel.DataAnnotations;

namespace CoffeeShop.Web.Models
{
    public class Dessert
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название обязательно.")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Цена обязательна.")]
        [Range(0.01, 10000.00, ErrorMessage = "Цена должна быть от 0.01 до 10000.")]
        public decimal Price { get; set; }

        [Display(Name = "Ссылка на изображение")]
        public string? ImageUrl { get; set; }
    }
}
