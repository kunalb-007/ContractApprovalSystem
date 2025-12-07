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

// CRITICAL FIX: Get connection string and add it to Configuration
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                      ?? Environment.GetEnvironmentVariable("DATABASE_URL")
                      ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("ERROR: No connection string found!");
    throw new Exception("Database connection string not configured");
}

// Inject connection string into configuration builder
builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
Console.WriteLine($"✓ Connection string set: {connectionString.Substring(0, 40)}...");

builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});

// Now DbContext will read from Configuration which we just set
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"DbContext using: {connStr?.Substring(0, 40) ?? "NULL"}...");

    if (string.IsNullOrEmpty(connStr))
    {
        throw new Exception("Connection string is null in DbContext configuration!");
    }

    options.UseNpgsql(connStr);
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
        var connStrTest = dbContext.Database.GetConnectionString();
        Console.WriteLine($"DbContext connection string: {connStrTest?.Substring(0, 40) ?? "NULL"}...");

        Console.WriteLine("Running migrations...");
        dbContext.Database.Migrate();
        Console.WriteLine("✓ Migrations completed");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Migration error: {ex.Message}");
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
