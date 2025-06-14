using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity; // Для связи с IdentityUser

namespace CoffeeShop.Web.Models
{
    public class LoyaltyPoints
    {
        public int Id { get; set; }

        // Пользователь, которому принадлежат баллы
        public string? UserId { get; set; } // ID пользователя из Identity (внешний ключ)
        public IdentityUser? User { get; set; } // Навигационное свойство к пользователю

        [Range(0, int.MaxValue, ErrorMessage = "Баллы лояльности не могут быть отрицательными.")]
        public int Points { get; set; } // Количество баллов

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow; // Дата последнего обновления
    }
}