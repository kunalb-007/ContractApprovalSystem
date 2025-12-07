using ContractApprovalSystem.Core.Interfaces;
using ContractApprovalSystem.Infrastructure.Data;
using ContractApprovalSystem.Infrastructure.Repositories;
using ContractApprovalSystem.Services.Interfaces;
using ContractApprovalSystem.Services.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Get PORT from environment (Render sets this to 10000)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Allow HTTP for Render
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrEmpty(connectionString))
    {
        Console.WriteLine("ERROR: Connection string is empty!");
        Console.WriteLine("Available env vars:");
        foreach (var env in Environment.GetEnvironmentVariables().Keys)
        {
            Console.WriteLine($"  {env}");
        }
    }
    else
    {
        Console.WriteLine($"Connection string found: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");
    }

    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IContractService, ContractService>();

var app = builder.Build();

// Auto-run migrations on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Console.WriteLine("Running database migrations...");
        dbContext.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration failed: {ex.Message}");
        Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Remove HTTPS redirect for Render (they handle SSL)
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

Console.WriteLine($"Starting app on port {port}");
app.Run();
