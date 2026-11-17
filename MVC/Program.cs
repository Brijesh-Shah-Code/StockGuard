using Microsoft.AspNetCore.Authentication.Cookies;
using MVC.BAL;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<AuthHelper>();

builder.Services.AddScoped<CategoriesHelper>();

builder.Services.AddScoped<SuppliersHelper>();

builder.Services.AddScoped<ProductsHelper>();

builder.Services.AddScoped<StockTransactionsHelper>();

builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(60);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

builder.Services.AddScoped<NpgsqlConnection>((ServiceProvider) =>
{
    var connectionString=ServiceProvider.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection");
    return new NpgsqlConnection(connectionString);
});


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
