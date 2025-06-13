using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Для атрибута [Authorize]
using CoffeeShop.Web.Data; // Для ApplicationDbContext
using Microsoft.EntityFrameworkCore; // Для ToListAsync, FindAsync, Include
using CoffeeShop.Web.Models; // Для моделей Coffee, Dessert, Category
using Microsoft.AspNetCore.Mvc.Rendering; // Для SelectList

namespace CoffeeShop.Web.Controllers
{
    [Authorize(Roles = "Administrator")] // Только администраторы имеют доступ к этому контроллеру
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
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
            // Передаем список категорий для выпадающего списка
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Добавление нового напитка
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCoffee([Bind("Name,Description,Price,ImageUrl,CategoryId")] Coffee coffee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(coffee);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Напиток успешно добавлен.";
                return RedirectToAction(nameof(Coffees));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", coffee.CategoryId);
            return View(coffee);
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", coffee.CategoryId);
            return View(coffee);
        }

        // POST: Редактирование напитка
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCoffee(int id, [Bind("Id,Name,Description,Price,ImageUrl,CategoryId")] Coffee coffee)
        {
            if (id != coffee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(coffee);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Напиток успешно обновлен.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CoffeeExists(coffee.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", coffee.CategoryId);
            return View(coffee);
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
            return View();
        }

        // POST: Добавление нового десерта
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDessert([Bind("Name,Description,Price,ImageUrl")] Dessert dessert)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dessert);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Десерт успешно добавлен.";
                return RedirectToAction(nameof(Desserts));
            }
            return View(dessert);
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
            return View(dessert);
        }

        // POST: Редактирование десерта
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDessert(int id, [Bind("Id,Name,Description,Price,ImageUrl")] Dessert dessert)
        {
            if (id != dessert.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dessert);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Десерт успешно обновлен.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DessertExists(dessert.Id))
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
            return View(dessert);
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
    }
}