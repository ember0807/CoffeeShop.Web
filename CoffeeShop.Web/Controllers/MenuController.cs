using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Для методов Include
using CoffeeShop.Web.Data; // Для ApplicationDbContext
using System.Linq; // Для LINQ запросов

namespace CoffeeShop.Web.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context; // Поле для доступа к базе данных

        // Конструктор контроллера, который получает ApplicationDbContext через Dependency Injection
        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Действие Index для отображения всего меню
        public async Task<IActionResult> Index()
        {
            // Загружаем напитки, включая связанные категории
            // .Include(c => c.Category) - это "жадная" загрузка, чтобы избежать N+1 проблемы
            var coffees = await _context.Coffees!.Include(c => c.Category).ToListAsync();
            var desserts = await _context.Desserts!.ToListAsync();

            // Для простоты, пока будем передавать напитки и десерты как отдельные данные
            // потом можно создать ViewModel для комбинированных данных
            ViewBag.Coffees = coffees;
            ViewBag.Desserts = desserts;

            return View(); // Возвращаем представление Index
        }

        // Действие для отображения деталей конкретного напитка
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Возвращает 404 ошибку, если ID не предоставлен
            }

            // Находим напиток по ID, включая категорию
            var coffee = await _context.Coffees!
                .Include(c => c.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (coffee == null)
            {
                return NotFound(); // Возвращает 404 ошибку, если напиток не найден
            }

            return View(coffee); // Передаем найденный напиток в представление
        }

        // Действие для отображения деталей конкретного десерта (аналогично Details для Coffee)
        public async Task<IActionResult> DessertDetails(int? id)
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
    }
}