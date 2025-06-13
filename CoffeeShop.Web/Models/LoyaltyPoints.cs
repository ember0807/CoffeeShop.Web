using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity; // Для использования IdentityUser

namespace CoffeeShop.Web.Models
{
    public class LoyaltyPoints
    {
        [Key]
        public int Id { get; set; }

        // UserId - это внешний ключ к таблице пользователей IdentityUser
        [Required]
        public string UserId { get; set; }

        // Навигационное свойство к пользователю.
        // IdentityUser - это базовая модель пользователя в ASP.NET Core Identity.
        public IdentityUser? User { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Баллы не могут быть отрицательными.")]
        public int Points { get; set; }
    }
}
