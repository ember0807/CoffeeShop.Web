using CoffeeShop.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("������ ����������� 'DefaultConnection' �� �������.");

// ������������ ApplicationDbContext � SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ����������� ASP.NET Core Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // �������� ��������� ����� � Identity
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ��������� ��������� ������������ � ��������������� (MVC)
builder.Services.AddControllersWithViews();
// ��������� ��������� Razor Pages (��� Identity UI)
builder.Services.AddRazorPages();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // ����� ������������ ������ �� � ���������
    options.Cookie.HttpOnly = true; // ���� ������ �������� ������ �� HTTP, �� ����� JavaScript
    options.Cookie.IsEssential = true; // ������� ���� ������ ������������� ��� ������ ����������
});

// --- ��������� �������� ����������� ---
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources"); // ���� � ����� � ������� ��������

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("ru-RU"),
        new CultureInfo("en-US")
    };

    options.DefaultRequestCulture = new RequestCulture("ru-RU"); // ������������� ������� �� ���������
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider()); // ����� ������������ ��������� Accept-Language
    
});

// ��������� MVC, ViewLocalization � DataAnnotationsLocalization � ����� ������
builder.Services.AddMvc()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix) // ��� ����������� �������������
    .AddDataAnnotationsLocalization(); // ��� ����������� ��������� ��������� � �������


var app = builder.Build();

// ��������� �������� � ��������� ������ ��� �������
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        // ������� ���� � ��������������
        await ApplicationDbContext.SeedRolesAndAdminUser(services);

        // ��������� ����� ��� ������� ���������� ����
        //await ApplicationDbContext.AssignAdminRoleToUser(services, "Ember0807@gmail.com");

    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService < ILogger < Program > >();
        logger.LogError(ex, "��������� ������ ��� ���������� ���� ������.");
    }
}

// �������� ��������� � ���������� ������ 
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

// --- ������������� ����������� 
app.UseRequestLocalization();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run(); 