using CoffeeShop.Web.Data; // Для ApplicationDbContext
using CoffeeShop.Web.Models; // Для моделей Coffee, Dessert, Category
using Microsoft.AspNetCore.Authorization; // Для атрибута [Authorize]
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Для SelectList
using Microsoft.EntityFrameworkCore; // Для ToListAsync, FindAsync, Include
using Microsoft.AspNetCore.Identity; // Для UserManager
using Microsoft.Extensions.Configuration; // Для доступа к appsettings.json
using System.IO; // Для работы с файловой системой
using CoffeeShop.Web.ViewModels; // Для новых ViewModel
using Microsoft.AspNetCore.Mvc.Rendering; // Для SelectList

namespace CoffeeShop.Web.Controllers
{
    [Authorize(Roles = "Administrator")] // Только администраторы имеют доступ к этому контроллеру
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly string _productImageUploadFolder; //Добавляем для пути загрузки
        //public AdminController(ApplicationDbContext context)
        //{
        //    _context = context;
        //}

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager; // <-- Инициализируем UserManager
            _configuration = configuration; // <-- Инициализируем IConfiguration
            _productImageUploadFolder = _configuration.GetValue<string>("ImageSettings:ProductImageUploadFolder") ?? "images/products/"; // <-- Получаем путь из appsettings.json
        }

        // Главная страница админ-панели
        public IActionResult Index()
        {
            return View(); // Создадим эту страницу позже
        }

        // --- Управление категориями ---

        // Отображение списка категорий
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories!.ToListAsync();
            return View(categories);
        }

        // GET: Добавление новой категории
        public IActionResult CreateCategory()
        {
            return View();
        }

        // POST: Добавление новой категории
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF атак
        public async Task<IActionResult> CreateCategory([Bind("Name")] Category category)
        {
            if (ModelState.IsValid) // Проверка валидации полей модели
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Категория успешно добавлена."; // Сообщение об успехе
                return RedirectToAction(nameof(Categories)); // Перенаправляем на список категорий
            }
            return View(category); // Если валидация не прошла, возвращаем форму с ошибками
        }

        // GET: Редактирование категории
        public async Task<IActionResult> EditCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories!.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Редактирование категории
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, [Bind("Id,Name")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Категория успешно обновлена.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        // GET: Удаление категории
        public async Task<IActionResult> DeleteCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories!
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Удаление категории
        [HttpPost, ActionName("DeleteCategory")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategoryConfirmed(int id)
        {
            var category = await _context.Categories!.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Категория успешно удалена.";
            return RedirectToAction(nameof(Categories));
        }

        private bool CategoryExists(int id)
        {
            return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
        }


        // --- Управление напитками (Coffee) ---

        // Отображение списка напитков
        public async Task<IActionResult> Coffees()
        {
            // Загружаем напитки вместе с их категориями
            var coffees = await _context.Coffees!.Include(c => c.Category).ToListAsync();
            return View(coffees);
        }

        // GET: Добавление нового напитка
        public IActionResult CreateCoffee()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            // Передаем пустую ViewModel
            return View(new CoffeeCreateEditViewModel()); // <--- Передаем ViewModel
        }

        // POST: Добавление нового напитка
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCoffee(CoffeeCreateEditViewModel model) // <--- Изменяем тип параметра
        {
            if (ModelState.IsValid)
            {
                string imageUrl = string.Empty;
                if (model.ImageFile != null)
                {
                    imageUrl = await UploadImage(model.ImageFile); // Используем ваш вспомогательный метод
                }

                var coffee = new Coffee
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    ImageUrl = imageUrl, // Сохраняем URL изображения
                    CategoryId = model.CategoryId
                };

                _context.Add(coffee);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Напиток успешно добавлен.";
                return RedirectToAction(nameof(Coffees));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            return View(model); // Возвращаем ViewModel с ошибками валидации
        }

        // GET: Редактирование напитка
        public async Task<IActionResult> EditCoffee(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coffee = await _context.Coffees!.FindAsync(id);
            if (coffee == null)
            {
                return NotFound();
            }

            // Преобразуем Coffee в CoffeeCreateEditViewModel для представления
            var viewModel = new CoffeeCreateEditViewModel
            {
                Id = coffee.Id,
                Name = coffee.Name,
                Description = coffee.Description,
                Price = coffee.Price,
                ExistingImageUrl = coffee.ImageUrl, // Передаем существующий URL для отображения
                CategoryId = coffee.CategoryId
            };

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", coffee.CategoryId);
            return View(viewModel); // <--- Передаем ViewModel
        }

        // POST: Редактирование напитка
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCoffee(int id, CoffeeCreateEditViewModel model) // <--- Изменяем тип параметра
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var coffeeToUpdate = await _context.Coffees!.FindAsync(id);
                    if (coffeeToUpdate == null)
                    {
                        return NotFound();
                    }

                    // Если загружено новое изображение, удаляем старое и сохраняем новое
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        DeleteImage(coffeeToUpdate.ImageUrl); // Удаляем старое
                        coffeeToUpdate.ImageUrl = await UploadImage(model.ImageFile); // Загружаем новое
                    }
                    // Обновляем остальные поля из ViewModel
                    coffeeToUpdate.Name = model.Name;
                    coffeeToUpdate.Description = model.Description;
                    coffeeToUpdate.Price = model.Price;
                    coffeeToUpdate.CategoryId = model.CategoryId;

                    _context.Update(coffeeToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Напиток успешно обновлен.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CoffeeExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Coffees));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            return View(model); // Возвращаем ViewModel с ошибками валидации
        }

        // GET: Удаление напитка
        public async Task<IActionResult> DeleteCoffee(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coffee = await _context.Coffees!
                .Include(c => c.Category) // Включаем категорию для отображения
                .FirstOrDefaultAsync(m => m.Id == id);
            if (coffee == null)
            {
                return NotFound();
            }

            return View(coffee);
        }

        // POST: Удаление напитка
        [HttpPost, ActionName("DeleteCoffee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCoffeeConfirmed(int id)
        {
            var coffee = await _context.Coffees!.FindAsync(id);
            if (coffee != null)
            {
                _context.Coffees.Remove(coffee);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Напиток успешно удален.";
            return RedirectToAction(nameof(Coffees));
        }

        private bool CoffeeExists(int id)
        {
            return (_context.Coffees?.Any(e => e.Id == id)).GetValueOrDefault();
        }


        // --- Управление десертами (Dessert) ---

        // Отображение списка десертов
        public async Task<IActionResult> Desserts()
        {
            var desserts = await _context.Desserts!.ToListAsync();
            return View(desserts);
        }

        // GET: Добавление нового десерта
        public IActionResult CreateDessert()
        {
            return View(new DessertCreateEditViewModel()); // Возвращаем пустую ViewModel
        }

        // POST: Добавление нового десерта
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDessert(DessertCreateEditViewModel model) // <--- Измените тип параметра
        {
            if (ModelState.IsValid)
            {
                string imageUrl = await UploadImage(model.ImageFile); // Используем ваш вспомогательный метод

                var dessert = new Dessert
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    ImageUrl = imageUrl // Сохраняем URL изображения
                };

                _context.Add(dessert);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Десерт успешно добавлен.";
                return RedirectToAction(nameof(Desserts));
            }
            return View(model); // Возвращаем модель с ошибками валидации
        }

        // GET: Редактирование десерта
        public async Task<IActionResult> EditDessert(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dessert = await _context.Desserts!.FindAsync(id);
            if (dessert == null)
            {
                return NotFound();
            }

            // Преобразуем Dessert в DessertCreateEditViewModel для представления
            var viewModel = new DessertCreateEditViewModel
            {
                Id = dessert.Id,
                Name = dessert.Name,
                Description = dessert.Description,
                Price = dessert.Price,
                ExistingImageUrl = dessert.ImageUrl // Передаем существующий URL для отображения
            };

            return View(viewModel); // <--- Передаем ViewModel в представление
        }



        // POST: Редактирование десерта
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDessert(int id, DessertCreateEditViewModel model) // <--- Измените тип параметра
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var dessertToUpdate = await _context.Desserts!.FindAsync(id);
                    if (dessertToUpdate == null)
                    {
                        return NotFound();
                    }

                    // Если загружено новое изображение, удаляем старое и сохраняем новое
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        DeleteImage(dessertToUpdate.ImageUrl); // Удаляем старое
                        dessertToUpdate.ImageUrl = await UploadImage(model.ImageFile); // Загружаем новое
                    }
                    // Обновляем остальные поля из ViewModel
                    dessertToUpdate.Name = model.Name;
                    dessertToUpdate.Description = model.Description;
                    dessertToUpdate.Price = model.Price;

                    _context.Update(dessertToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Десерт успешно обновлен.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DessertExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Desserts));
            }
            return View(model); // Возвращаем модель с ошибками валидации
        }

        // GET: Удаление десерта
        public async Task<IActionResult> DeleteDessert(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dessert = await _context.Desserts!
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dessert == null)
            {
                return NotFound();
            }

            return View(dessert);
        }

        // POST: Удаление десерта
        [HttpPost, ActionName("DeleteDessert")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDessertConfirmed(int id)
        {
            var dessert = await _context.Desserts!.FindAsync(id);
            if (dessert != null)
            {
                _context.Desserts.Remove(dessert);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Десерт успешно удален.";
            return RedirectToAction(nameof(Desserts));
        }

        private bool DessertExists(int id)
        {
            return (_context.Desserts?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // GET: Admin/Orders - Список всех заказов
        public async Task<IActionResult> Orders()
        {
            // Получаем все заказы, включая их элементы (OrderItems) и информацию о пользователе (если есть)
            var orders = await _context.Orders!
                                       .Include(o => o.OrderItems)
                                       .Include(o => o.User) // Загружаем связанного пользователя
                                       .OrderByDescending(o => o.OrderDate) // Сортируем по дате, новые сверху
                                       .ToListAsync();
            return View(orders);
        }
        // GET: Admin/OrderDetails/5 - Детали конкретного заказа
        public async Task<IActionResult> OrderDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders!
                                      .Include(o => o.OrderItems) // Загружаем все элементы заказа
                                      .Include(o => o.User) // Загружаем связанного пользователя
                                      .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        // POST: Admin/UpdateOrderStatus - Обновление статуса заказа
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, string newStatus)
        {
            var order = await _context.Orders!.FindAsync(id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Заказ не найден.";
                return RedirectToAction(nameof(Orders));
            }

            // Добавляем простую валидацию статуса (можно расширить)
            var validStatuses = new List<string> { "Pending", "Processing", "Completed", "Cancelled" };
            if (!validStatuses.Contains(newStatus))
            {
                TempData["ErrorMessage"] = "Недопустимый статус заказа.";
                return RedirectToAction(nameof(Orders));
            }

            order.Status = newStatus;
            try
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Статус заказа №{order.Id} успешно обновлен на '{newStatus}'.";
            }
            catch (DbUpdateConcurrencyException)
            {
                // Обработка конфликтов параллельного доступа (если нужно)
                TempData["ErrorMessage"] = "Произошла ошибка при обновлении статуса заказа. Попробуйте еще раз.";
            }

            return RedirectToAction(nameof(OrderDetails), new { id = order.Id });
        }
        // Метод для загрузки изображения
        private async Task<string> UploadImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return string.Empty;
            }

            // Генерируем уникальное имя файла
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", _productImageUploadFolder);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Убедитесь, что папка существует
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // Возвращаем путь к файлу относительно wwwroot
            return $"/{_productImageUploadFolder}{uniqueFileName}";
        }
        // Метод для удаления изображения
        private void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return;
            }

            // Удаляем ведущий слэш, если он есть
            var relativePath = imageUrl.TrimStart('/');
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}