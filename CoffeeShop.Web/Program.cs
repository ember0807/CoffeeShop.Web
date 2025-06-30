using CoffeeShop.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Ñòðîêà ïîäêëþ÷åíèÿ 'DefaultConnection' íå íàéäåíà.");

// Ðåãèñòðèðóåì ApplicationDbContext ñ SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Íàñòðàèâàåì ASP.NET Core Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // Âêëþ÷àåì ïîääåðæêó ðîëåé â Identity
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Äîáàâëÿåì ïîääåðæêó êîíòðîëëåðîâ ñ ïðåäñòàâëåíèÿìè (MVC)
builder.Services.AddControllersWithViews();
// Äîáàâëÿåì ïîääåðæêó Razor Pages (äëÿ Identity UI)
builder.Services.AddRazorPages();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Âðåìÿ íåàêòèâíîñòè ñåññèè äî å¸ èñòå÷åíèÿ
    options.Cookie.HttpOnly = true; // Êóêè ñåññèè äîñòóïíû òîëüêî ïî HTTP, íå ÷åðåç JavaScript
    options.Cookie.IsEssential = true; // Ñäåëàòü êóêè ñåññèè îáÿçàòåëüíûìè äëÿ ðàáîòû ïðèëîæåíèÿ
});

// --- Íàñòðîéêà ñåðâèñîâ ëîêàëèçàöèè ---
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources"); // Ïóòü ê ïàïêå ñ ôàéëàìè ðåñóðñîâ

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("ru-RU"),
        new CultureInfo("en-US")
    };

    options.DefaultRequestCulture = new RequestCulture("ru-RU"); // Óñòàíàâëèâàåì ðóññêèé ïî óìîë÷àíèþ
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider()); // Ìîæíî èñïîëüçîâàòü çàãîëîâîê Accept-Language
    
});

// Äîáàâëÿåì MVC, ViewLocalization è DataAnnotationsLocalization â îäíîì âûçîâå
builder.Services.AddMvc()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix) // Äëÿ ëîêàëèçàöèè ïðåäñòàâëåíèé
    .AddDataAnnotationsLocalization(); // Äëÿ ëîêàëèçàöèè àòðèáóòîâ âàëèäàöèè â ìîäåëÿõ


var app = builder.Build();

// Ïðèìåíÿåì ìèãðàöèè è çàïîëíÿåì äàííûå ïðè çàïóñêå
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        // Ñîçäàåì ðîëè è àäìèíèñòðàòîðà
        await ApplicationDbContext.SeedRolesAndAdminUser(services);

        // ÂÐÅÌÅÍÍÛÉ ÂÛÇÎÂ ÄËß ÐÓ×ÍÎÃÎ ÍÀÇÍÀ×ÅÍÈß ÐÎËÈ
        //await ApplicationDbContext.AssignAdminRoleToUser(services, "...@gmail.com");

    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService < ILogger < Program > >();
        logger.LogError(ex, "Ïðîèçîøëà îøèáêà ïðè çàïîëíåíèè áàçû äàííûõ.");
    }
}

// Ïðîâåðêà îêðóæåíèÿ è îáðàáîò÷èê îøèáîê 
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

// --- ÈÑÏÎËÜÇÎÂÀÍÈÅ ËÎÊÀËÈÇÀÖÈÈ 
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
