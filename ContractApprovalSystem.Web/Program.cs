using Microsoft.EntityFrameworkCore;
using ContractApprovalSystem.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Required fix for Render IPv6 issue
AppContext.SetSwitch("Npgsql.EnableConfigurableDnsResolution", true);

// 1️⃣ Try to get DATABASE_URL (Supabase connection URL)
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

// 2️⃣ If DATABASE_URL exists → convert to Npgsql connection string
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    var uri = new Uri(databaseUrl);

    var userInfo = uri.UserInfo.Split(':');
    var username = userInfo[0];
    var password = userInfo[1];

    connectionString =
        $"Host={uri.Host};" +
        $"Port={uri.Port};" +
        $"Database={uri.AbsolutePath.TrimStart('/')};" +
        $"Username={username};" +
        $"Password={password};" +
        $"SSL Mode=Require;Trust Server Certificate=true";
}
else
{
    // 3️⃣ Fallback for local development (from appsettings.json)
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

// 4️⃣ Configure EF
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
