using ContractApprovalSystem.Core.Interfaces;
using ContractApprovalSystem.Infrastructure.Data;
using ContractApprovalSystem.Infrastructure.Repositories;
using ContractApprovalSystem.Services.Interfaces;
using ContractApprovalSystem.Services.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// PORT for RENDER
// -------------------------
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// -------------------------
// GET RAW CONNECTION STRING
// -------------------------
var rawConn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
             ?? Environment.GetEnvironmentVariable("DATABASE_URL")
             ?? builder.Configuration.GetConnectionString("DefaultConnection");


if (string.IsNullOrEmpty(rawConn))
{
    Console.WriteLine("ERROR: No connection string found!");
    throw new Exception("Database connection string not configured");
    throw new Exception("Database connection string not configured.");
}

Console.WriteLine($"✓ Raw connection string received: {rawConn.Substring(0, 40)}...");

// -------------------------
// CONVERT IF URL FORMAT (Render gives: postgresql://...)
// -------------------------
string finalConn;

if (rawConn.StartsWith("postgres://") || rawConn.StartsWith("postgresql://"))
{
    var uri = new Uri(rawConn);
    var userInfo = uri.UserInfo.Split(':');

    var username = userInfo[0];
    var password = userInfo.Length > 1 ? userInfo[1] : "";

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
    // Already EF format
    finalConn = rawConn;
}

Console.WriteLine($"✓ Final EF connection string: {finalConn.Substring(0, 40)}...");

// Inject into config so DbContext sees it
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

// DB CONTEXT
 builder.Services.AddDbContext<ApplicationDbContext>(options =>
 {
     var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
     Console.WriteLine($"DbContext using: {connStr?.Substring(0, 40) ?? "NULL"}...");

     if (string.IsNullOrEmpty(connStr))
     {
         throw new Exception("Connection string is null in DbContext configuration!");
     }

     options.UseNpgsql(connStr);
     Console.WriteLine($"DbContext using: {finalConn.Substring(0, 40)}...");
     options.UseNpgsql(finalConn);
 });


// Dependency Injections
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
         Console.WriteLine("✓ Migrations completed successfully");
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