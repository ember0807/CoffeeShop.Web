using Microsoft.AspNetCore.Http; // Для IFormFile
using System.ComponentModel.DataAnnotations;

namespace CoffeeShop.Web.ViewModels
{
    public class DessertCreateEditViewModel
    {
        public int Id { get; set; } // Для редактирования

        [Required(ErrorMessage = "Название десерта обязательно.")]
        [StringLength(100, ErrorMessage = "Название не может превышать 100 символов.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Описание не может превышать 500 символов.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Цена обязательна.")]
        [Range(0.01, 10000.00, ErrorMessage = "Цена должна быть от 0.01 до 10000.")]
        public decimal Price { get; set; }

        // Это свойство будет использоваться для загрузки нового файла изображения
        [Display(Name = "Изображение")]
        public IFormFile? ImageFile { get; set; } 

        // Это свойство будет использоваться для отображения существующего URL изображения при редактировании
        public string? ExistingImageUrl { get; set; }
    }
}