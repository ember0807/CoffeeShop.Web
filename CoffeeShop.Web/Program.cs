using CoffeeShop.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;

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
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "��������� ������ ��� ���������� ���� ������.");
    }
}

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

app.UseAuthorization();
app.UseAuthentication();

app.UseSession();//��� ����� � �������

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
