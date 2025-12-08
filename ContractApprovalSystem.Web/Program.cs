using ContractApprovalSystem.Core.Interfaces;
using ContractApprovalSystem.Infrastructure.Data;
using ContractApprovalSystem.Infrastructure.Repositories;
using ContractApprovalSystem.Services.Interfaces;
using ContractApprovalSystem.Services.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// PORT for Render
// -------------------------
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// -------------------------
// GET RAW CONNECTION STRING
// -------------------------
var rawConn =
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(rawConn))
{
    Console.WriteLine("ERROR: No database connection string found!");
    throw new Exception("DATABASE_URL or DefaultConnection is missing.");
}

// safe preview for logs
string SafePreview(string s)
{
    if (string.IsNullOrEmpty(s)) return "EMPTY";

    if (s.Length <= 15) return s; // short
    return s[..15] + "...(masked)";
}

Console.WriteLine($"✓ Raw connection string detected: {SafePreview(rawConn)}");

// -------------------------
// CONVERT postgres:// URL → Npgsql format
// -------------------------
string finalConn;

bool IsUrlStyle = rawConn.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
               || rawConn.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);

if (IsUrlStyle)
{
    var uri = new Uri(rawConn);

    // Split username:password — password may contain ':' or '@'
    var userInfo = uri.UserInfo;
    var splitIndex = userInfo.IndexOf(':');

    var username = splitIndex > 0 ? userInfo[..splitIndex] : userInfo;
    var password = splitIndex > 0 ? userInfo[(splitIndex + 1)..] : "";

    finalConn =
        $"Host={uri.Host};" +
        $"Port={(uri.Port > 0 ? uri.Port : 5432)};" +
        $"Database={uri.AbsolutePath.TrimStart('/')};" +
        $"Username={username};" +
        $"Password={password};" +
        $"SSL Mode=Require;Trust Server Certificate=true;";
}
else
{
    finalConn = rawConn; // already EF format
}

Console.WriteLine($"✓ Final EF-ready connection string: {SafePreview(finalConn)}");

// Inject into config for DbContext
builder.Configuration["ConnectionStrings:DefaultConnection"] = finalConn;

// -------------------------
// SERVICES
// -------------------------
builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});

// DB Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    Console.WriteLine($"DbContext using: {SafePreview(finalConn)}");

    options.UseNpgsql(finalConn);
});

// DI
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IContractService, ContractService>();

var app = builder.Build();

// -------------------------
// RUN MIGRATIONS ON START
// -------------------------
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Console.WriteLine("Running migrations...");
        db.Database.Migrate();
        Console.WriteLine("✓ Migrations completed");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Migration error: {ex.Message}");
    }
}

// -------------------------
// PIPELINE
// -------------------------
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
