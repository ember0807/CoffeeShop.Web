using Microsoft.AspNetCore.Mvc;
using CoffeeShop.Web.Data;
using CoffeeShop.Web.Models;
using CoffeeShop.Web.ViewModels; // Для CartItem
using CoffeeShop.Web.Extensions; // Для SessionExtensions
using Microsoft.EntityFrameworkCore; // Для Include, FindAsync
using Microsoft.AspNetCore.Identity; // Для UserManager

namespace CoffeeShop.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager; // Добавляем UserManager
        private const string CartSessionKey = "Cart"; // Ключ для хранения корзины в сессии

        public CartController(ApplicationDbContext context, UserManager<IdentityUser> userManager) // Внедряем UserManager
        {
            _context = context;
            _userManager = userManager; // Инициализируем UserManager
        }

        // Отображение содержимого корзины
        public IActionResult Index()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            return View(cart);
        }

        // Добавление товара в корзину
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, string productType, int quantity = 1)
        {
            if (quantity <= 0)
            {
                TempData["ErrorMessage"] = "Количество товара должно быть больше нуля.";
                return RedirectToAction("Index", "Menu");
            }

            var cart = HttpContext.Session.Get<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            CartItem? existingItem = null; // Объявляем здесь, чтобы использовать ниже

            if (productType == "Coffee")
            {
                var coffee = await _context.Coffees!.FindAsync(productId);
                if (coffee == null)
                {
                    TempData["ErrorMessage"] = "Кофе не найден.";
                    return RedirectToAction("Index", "Menu");
                }
                // Проверяем существующий элемент по ProductId И ProductType
                existingItem = cart.FirstOrDefault(item => item.ProductId == productId && item.ProductType == "Coffee");

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = coffee.Id,
                        ProductName = coffee.Name,
                        Price = coffee.Price,
                        Quantity = quantity,
                        ImageUrl = coffee.ImageUrl,
                        ProductType = "Coffee"
                    });
                }
            }
            else if (productType == "Dessert")
            {
                var dessert = await _context.Desserts!.FindAsync(productId);
                if (dessert == null)
                {
                    TempData["ErrorMessage"] = "Десерт не найден.";
                    return RedirectToAction("Index", "Menu");
                }
                // Проверяем существующий элемент по ProductId И ProductType
                existingItem = cart.FirstOrDefault(item => item.ProductId == productId && item.ProductType == "Dessert");

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = dessert.Id,
                        ProductName = dessert.Name,
                        Price = dessert.Price,
                        Quantity = quantity,
                        ImageUrl = dessert.ImageUrl,
                        ProductType = "Dessert"
                    });
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Неизвестный тип продукта.";
                return RedirectToAction("Index", "Menu");
            }
            HttpContext.Session.Set(CartSessionKey, cart);

            // Улучшенное сообщение, чтобы видеть название добавленного товара
            string addedProductName = existingItem?.ProductName ??
                                      (productType == "Coffee" ? (await _context.Coffees!.FindAsync(productId))?.Name :
                                      (productType == "Dessert" ? (await _context.Desserts!.FindAsync(productId))?.Name : ""));

            TempData["SuccessMessage"] = $"Товар '{addedProductName}' добавлен в корзину.";
            return RedirectToAction("Index", "Cart");
        }
        // Обновление количества товара в корзине
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, string productType, int quantity)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var itemToUpdate = cart.FirstOrDefault(item => item.ProductId == productId && item.ProductType == productType);

            if (itemToUpdate != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(itemToUpdate); // Удаляем, если количество <= 0
                    TempData["SuccessMessage"] = $"Товар '{itemToUpdate.ProductName}' удален из корзины.";
                }
                else
                {
                    itemToUpdate.Quantity = quantity;
                    TempData["SuccessMessage"] = $"Количество товара '{itemToUpdate.ProductName}' обновлено.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Товар не найден в корзине.";
            }

            HttpContext.Session.Set(CartSessionKey, cart);
            return RedirectToAction(nameof(Index));
        }

        // Удаление товара из корзины
        [HttpPost]
        public IActionResult RemoveFromCart(int productId, string productType)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var itemToRemove = cart.FirstOrDefault(item => item.ProductId == productId && item.ProductType == productType);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                HttpContext.Session.Set(CartSessionKey, cart);
                TempData["SuccessMessage"] = $"Товар '{itemToRemove.ProductName}' удален из корзины.";
            }
            else
            {
                TempData["ErrorMessage"] = "Товар не найден в корзине.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Очистка корзины
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
            TempData["SuccessMessage"] = "Корзина очищена.";
            return RedirectToAction(nameof(Index));
        }
        // GET: Отображение формы оформления заказа
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Ваша корзина пуста. Добавьте товары, чтобы оформить заказ.";
                return RedirectToAction("Index", "Cart");
            }

            var model = new Order(); // Создаем пустую модель заказа

            // Если пользователь авторизован, предварительно заполняем поля
            if (User.Identity!.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    model.UserId = user.Id;
                    model.CustomerEmail = user.Email ?? "";
                    model.CustomerName = user.UserName ?? ""; // Можно использовать UserName как имя
                    // CustomerPhone и DeliveryAddress могут быть из профиля, но пока нет профиля
                }
            }

            model.TotalAmount = cart.Sum(item => item.TotalPrice);
            return View(model);
        }

        // POST: Обработка оформления заказа
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout([Bind("CustomerName,CustomerEmail,CustomerPhone,DeliveryAddress")] Order order)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Ваша корзина пуста. Невозможно оформить заказ.";
                return RedirectToAction("Index", "Cart");
            }

            order.TotalAmount = cart.Sum(item => item.TotalPrice);
            order.OrderDate = DateTime.UtcNow;
            order.Status = "Pending"; // Начальный статус заказа

            // Если пользователь авторизован, привязываем заказ к его UserId
            if (User.Identity!.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                order.UserId = user?.Id;
            }

            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync(); // Сохраняем заказ, чтобы получить его Id

                // Добавляем элементы корзины как OrderItem'ы
                foreach (var item in cart)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ProductType = item.ProductType,
                        Price = item.Price,
                        Quantity = item.Quantity
                    };
                    _context.Add(orderItem);
                }
                await _context.SaveChangesAsync(); // Сохраняем элементы заказа

                // Обновляем баллы лояльности
                if (User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        await CalculateLoyaltyPoints(user.Id, order.TotalAmount);
                    }
                }

                HttpContext.Session.Remove(CartSessionKey); // Очищаем корзину после оформления заказа

                TempData["SuccessMessage"] = $"Ваш заказ №{order.Id} успешно оформлен! Ожидайте звонка менеджера.";
                return RedirectToAction("OrderConfirmation", new { id = order.Id }); // Перенаправление на страницу подтверждения
            }

            // Если валидация не прошла, возвращаем форму с ошибками
            // Собираем сообщения об ошибках из ModelState
            var errorMessages = new List<string>();
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key]!.Errors;
                if (errors.Any())
                {
                    foreach (var error in errors)
                    {
                        errorMessages.Add($"Ошибка в поле '{key}': {error.ErrorMessage}");
                    }
                }
            }

            // Сохраняем сообщения об ошибках в TempData
            TempData["ErrorMessages"] = errorMessages;

            return View(order);
        }

        // Метод для расчета и обновления баллов лояльности
        private async Task CalculateLoyaltyPoints(string userId, decimal orderTotal)
        {
            // Пример: 1 балл за каждые 100 рублей
            int pointsEarned = (int)Math.Floor(orderTotal / 100);

            var loyaltyPoints = await _context.LoyaltyPoints!
                .FirstOrDefaultAsync(lp => lp.UserId == userId);

            if (loyaltyPoints == null)
            {
                // Если записи нет, создаем новую
                loyaltyPoints = new LoyaltyPoints
                {
                    UserId = userId,
                    Points = pointsEarned,
                    LastUpdated = DateTime.UtcNow
                };
                _context.Add(loyaltyPoints);
            }
            else
            {
                // Если запись есть, обновляем баллы
                loyaltyPoints.Points += pointsEarned;
                loyaltyPoints.LastUpdated = DateTime.UtcNow;
                _context.Update(loyaltyPoints);
            }

            await _context.SaveChangesAsync();
            TempData["LoyaltyPointsMessage"] = $"Вы заработали {pointsEarned} баллов лояльности! Всего баллов: {loyaltyPoints.Points}";
        }

        // Страница подтверждения заказа (после успешного оформления)
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _context.Orders!
                            .Include(o => o.OrderItems)
                            .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Дополнительная проверка, что пользователь имеет право видеть этот заказ (если авторизован)
            if (User.Identity!.IsAuthenticated && order.UserId != _userManager.GetUserId(User))
            {
                // Не админ и не владелец заказа
                TempData["ErrorMessage"] = "У вас нет доступа к этому заказу.";
                return RedirectToAction("Index", "Home");
            }
            else if (!User.Identity.IsAuthenticated)
            {
                // Если пользователь не авторизован, заказ мог быть оформлен как гость.

            }

            return View(order);
        }

    }
}
