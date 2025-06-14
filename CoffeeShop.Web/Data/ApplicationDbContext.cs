using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Web.Models; // Добавляем using для наших моделей
using Microsoft.AspNetCore.Identity; // для IdentityUser и UserManager
using System; // для DateTime
using System.Linq;

namespace CoffeeShop.Web.Data
{
    // ApplicationDbContext наследуется от IdentityDbContext, который уже содержит
    // все необходимые сущности для работы с Identity (пользователи, роли и т.д.).
    public class ApplicationDbContext : IdentityDbContext
    {
        // Конструктор, который получает опции контекста из DI-контейнера
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet<T> представляет собой коллекцию сущностей в контексте,
        // которые могут быть запрошены из базы данных и сохранены в ней.
        // Каждое DbSet будет соответствовать таблице в вашей базе данных.
        public DbSet<Coffee>? Coffees { get; set; }
        public DbSet<Dessert>? Desserts { get; set; }
        public DbSet<Category>? Categories { get; set; }
        // DbSet для IdentityUser уже есть в IdentityDbContext

        // Новые DbSet для Order, OrderItem и LoyaltyPoints
        public DbSet<Order>? Orders { get; set; }
        public DbSet<OrderItem>? OrderItems { get; set; }
        public DbSet<LoyaltyPoints>? LoyaltyPoints { get; set; }


        // Метод OnModelCreating используется для дополнительной настройки модели,
        // например, для определения связей между сущностями.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Важно вызвать базовый метод IdentityDbContext

            // Настройка связей (Relationships)
            // Order <-> OrderItem (один заказ ко многим элементам заказа)
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении заказа удаляются и его элементы

            // Order <-> IdentityUser (один пользователь ко многим заказам)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany() // У IdentityUser нет навигационного свойства для заказов, поэтому Many()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.SetNull); // При удалении пользователя UserId в Order станет NULL

            // LoyaltyPoints <-> IdentityUser (один пользователь к одной записи баллов)
            modelBuilder.Entity<LoyaltyPoints>()
                .HasOne(lp => lp.User)
                .WithMany() // У IdentityUser нет навигационного свойства для LoyaltyPoints
                .HasForeignKey(lp => lp.UserId)
                .IsRequired() // Связь обязательна
                .OnDelete(DeleteBehavior.Cascade); // При удалении пользователя удаляются и его баллы

            // Убедитесь, что LoyaltyPoints.UserId уникален (один пользователь = одна запись баллов)
            modelBuilder.Entity<LoyaltyPoints>()
                .HasIndex(lp => lp.UserId)
                .IsUnique();


            // Fluent API для настройки связи один-ко-многим между Category и Coffee
            modelBuilder.Entity<Coffee>()
                .HasOne(c => c.Category) // У Coffee есть одна Category
                .WithMany(cat => cat.Coffees) // Category может иметь много Coffees
                .HasForeignKey(c => c.CategoryId) // Внешний ключ в таблице Coffee
                                                  // OnDelete(DeleteBehavior.Cascade) - при удалении категории,
                                                  // все связанные с ней напитки также будут удалены.
                .OnDelete(DeleteBehavior.Cascade);

            // Добавление начальных данных для категорий
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Кофе" },
                new Category { Id = 2, Name = "Чай" },
                new Category { Id = 3, Name = "Выпечка" },
                new Category { Id = 4, Name = "Десерты" }
            );

            // Добавление начальных данных для кофе
            modelBuilder.Entity<Coffee>().HasData(
                new Coffee { Id = 1, Name = "Эспрессо", Description = "Классический крепкий кофе.", Price = 150.00m, ImageUrl = "/images/espresso.jpg", CategoryId = 1 },
                new Coffee { Id = 2, Name = "Латте", Description = "Нежный кофе с молоком.", Price = 250.00m, ImageUrl = "/images/latte.jpg", CategoryId = 1 },
                new Coffee { Id = 3, Name = "Капучино", Description = "Кофе с пышной молочной пенкой.", Price = 230.00m, ImageUrl = "/images/cappuccino.jpg", CategoryId = 1 },
                new Coffee { Id = 4, Name = "Зеленый чай", Description = "Бодрящий зеленый чай.", Price = 180.00m, ImageUrl = "/images/green_tea.jpg", CategoryId = 2 }
            );

            // Добавление начальных данных для десертов
            modelBuilder.Entity<Dessert>().HasData(
                new Dessert { Id = 1, Name = "Чизкейк", Description = "Классический Нью-Йоркский чизкейк.", Price = 300.00m, ImageUrl = "/images/cheesecake.jpg" },
                new Dessert { Id = 2, Name = "Круассан", Description = "Свежий французский круассан.", Price = 120.00m, ImageUrl = "/images/croissant.jpg" }
            );

        }
        // Новый метод для добавления ролей и первого администратора
        public static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Создаем роль "Administrator", если она не существует
            if (!await roleManager.RoleExistsAsync("Administrator"))
            {
                await roleManager.CreateAsync(new IdentityRole("Administrator"));
            }

            // Создаем роль "User", если она не существует
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Создаем первого администратора, если его нет
            var adminUser = await userManager.FindByEmailAsync("admin@coffeeshop.com");
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = "admin@coffeeshop.com",
                    Email = "admin@coffeeshop.com",
                    EmailConfirmed = true // Подтверждаем email для простоты
                };
                await userManager.CreateAsync(adminUser, "AdminPassword123!");
                await userManager.AddToRoleAsync(adminUser, "Administrator");
            }
        }
        // ВРЕМЕННЫЙ МЕТОД ДЛЯ РУЧНОГО НАЗНАЧЕНИЯ РОЛИ
        public static async Task AssignAdminRoleToUser(IServiceProvider serviceProvider, string userEmail)
        {
            var userManager = serviceProvider.GetRequiredService < UserManager < IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService < RoleManager < IdentityRole>>();

            Console.WriteLine($"Попытка назначить роль 'Administrator' пользователю: {userEmail}");

            // 1. Убедимся, что роль "Administrator" существует
            string adminRoleName = "Administrator";
            if (!await roleManager.RoleExistsAsync(adminRoleName))
            {
                Console.WriteLine($"Роль '{adminRoleName}' не найдена. Создаем...");
                await roleManager.CreateAsync(new IdentityRole(adminRoleName));
                Console.WriteLine($"Роль '{adminRoleName}' создана.");
            }
            else
            {
                Console.WriteLine($"Роль '{adminRoleName}' уже существует.");
            }

            // 2. Найдем пользователя по email
            var user = await userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                Console.WriteLine($"Пользователь с email '{userEmail}' не найден. Убедитесь, что он зарегистрирован.");
            }
            else
            {
                // 3. Проверим, состоит ли пользователь уже в роли "Administrator"
                if (await userManager.IsInRoleAsync(user, adminRoleName))
                {
                    Console.WriteLine($"Пользователь '{userEmail}' уже является администратором.");
                }
                else
                {
                    // 4. Добавим пользователя в роль "Administrator"
                    var result = await userManager.AddToRoleAsync(user, adminRoleName);

                    if (result.Succeeded)
                    {
                        Console.WriteLine($"Пользователь '{userEmail}' успешно назначен администратором!");
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка при назначении роли администратора пользователю '{userEmail}':");
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"- {error.Description}");
                        }
                    }
                }
            }
        }
    }



}


