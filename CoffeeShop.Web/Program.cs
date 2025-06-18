using CoffeeShop.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Строка подключения 'DefaultConnection' не найдена.");

// Регистрируем ApplicationDbContext с SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Настраиваем ASP.NET Core Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // Включаем поддержку ролей в Identity
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Добавляем поддержку контроллеров с представлениями (MVC)
builder.Services.AddControllersWithViews();
// Добавляем поддержку Razor Pages (для Identity UI)
builder.Services.AddRazorPages();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Время неактивности сессии до её истечения
    options.Cookie.HttpOnly = true; // Куки сессии доступны только по HTTP, не через JavaScript
    options.Cookie.IsEssential = true; // Сделать куки сессии обязательными для работы приложения
});

// --- Настройка сервисов локализации (ДОБАВИТЬ ЭТОТ БЛОК) ---
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources"); // Укажите путь к папке с файлами ресурсов
builder.Services.AddMvc()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix) // Для локализации представлений
    .AddDataAnnotationsLocalization(); // Для локализации атрибутов валидации в моделях
// --- КОНЕЦ БЛОКА НАСТРОЙКИ СЕРВИСОВ ЛОКАЛИЗАЦИИ ---

var app = builder.Build();

// Применяем миграции и заполняем данные при запуске
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        // Создаем роли и администратора
        await ApplicationDbContext.SeedRolesAndAdminUser(services);

        // ВРЕМЕННЫЙ ВЫЗОВ ДЛЯ РУЧНОГО НАЗНАЧЕНИЯ РОЛИ
        //await ApplicationDbContext.AssignAdminRoleToUser(services, "Ember0807@gmail.com");

    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Произошла ошибка при заполнении базы данных.");
    }
}

// --- Настройка поддерживаемых языов  ---
var defaultCulture = "ru-RU"; // Устанавливаем русский по умолчанию
var supportedCultures = new[] {
    new CultureInfo(defaultCulture),
    new CultureInfo("en-US") // Можно оставить, на всякий случай
};
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});


if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.UseSession();//для сесии в корзину

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();