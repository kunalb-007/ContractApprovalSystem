using ContractApprovalSystem.Core.Interfaces;
using ContractApprovalSystem.Infrastructure.Data;
using ContractApprovalSystem.Infrastructure.Repositories;
using ContractApprovalSystem.Services.Interfaces;
using ContractApprovalSystem.Services.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Get PORT
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// CRITICAL: Get connection string BEFORE building anything
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
}

if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("FATAL: No database connection string found!");
}

Console.WriteLine($"✓ Connection string captured: {connectionString.Substring(0, 40)}...");

builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});

// Add DbContext with the connection string captured above
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    Console.WriteLine($"Configuring DbContext with: {connectionString.Substring(0, 40)}...");
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IContractService, ContractService>();

var app = builder.Build();

// Auto-run migrations
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Console.WriteLine("Running migrations...");
        dbContext.Database.Migrate();
        Console.WriteLine("✓ Migrations completed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Migration failed: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        // Don't throw - let app start anyway
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

Console.WriteLine($"✓ App started on port {port}");
app.Run();
