using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // Для IFormFile
using Microsoft.AspNetCore.Mvc.Rendering; // Для SelectList

namespace CoffeeShop.Web.ViewModels
{
    public class CoffeeCreateEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Поле 'Название' обязательно.")]
        [StringLength(100, ErrorMessage = "Название не может превышать 100 символов.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Описание не может превышать 500 символов.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Поле 'Цена' обязательно.")]
        [Range(0.01, 10000.00, ErrorMessage = "Цена должна быть от 0.01 до 10000.")]
        public decimal Price { get; set; }

        public string? ExistingImageUrl { get; set; } // Для отображения существующего изображения при редактировании

        [Display(Name = "Изображение")]
        public IFormFile? ImageFile { get; set; } // Для загрузки нового изображения

        [Required(ErrorMessage = "Поле 'Категория' обязательно.")]
        [Display(Name = "Категория")]
        public int CategoryId { get; set; }

        public IEnumerable<SelectListItem>? Categories { get; set; } // Для выпадающего списка категорий
    }
}