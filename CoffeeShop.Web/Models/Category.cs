using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace CoffeeShop.Web.Models
{
    public class Category
    {
        [Key] // Указывает, что это первичный ключ таблицы
        public int Id { get; set; }

        [Required(ErrorMessage = "Название категории обязательно.")] // Поле обязательно для заполнения
        [StringLength(50, ErrorMessage = "Название категории не должно превышать 50 символов.")] 
        [Display(Name = "Название категории")] // Отображаемое имя для UI
        public string Name { get; set; }

        // Навигационное свойство: одна категория может иметь много напитков.
        // "?" делает коллекцию обнуляемой, чтобы избежать предупреждений.
        public ICollection<Coffee>? Coffees { get; set; }
    }
}
