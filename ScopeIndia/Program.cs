using Microsoft.EntityFrameworkCore;
using ScopeIndia.Data;
using ScopeIndia.Services;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------------
// ⿡ Add MVC Controllers & Views
// --------------------------------------------------------
builder.Services.AddControllersWithViews();

// --------------------------------------------------------
// ⿢ Configure SQL Server Database
// --------------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --------------------------------------------------------
// ⿣ Register Email Service (for OTP sending)
// --------------------------------------------------------
builder.Services.AddScoped<IEmailService, EmailService>();

// --------------------------------------------------------
// ⿤ Configure Session Management
// --------------------------------------------------------
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout duration
    options.Cookie.HttpOnly = true;                 // Protect cookie from client-side JS
    options.Cookie.IsEssential = true;              // Required for GDPR compliance
});

// --------------------------------------------------------
// ⿥ Build the Application
// --------------------------------------------------------
var app = builder.Build();

// --------------------------------------------------------
// ⿦ Error Handling & Security Middleware
// --------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// --------------------------------------------------------
// ⿧ Core Middlewares
// --------------------------------------------------------
app.UseHttpsRedirection();  // Redirect HTTP -> HTTPS
app.UseStaticFiles();       // Enable serving static files (CSS, JS, Images)
app.UseRouting();           // Enable endpoint routing
app.UseSession();           // Enable Session Middleware
app.UseAuthorization();     // Enable authorization (if you use [Authorize])

// --------------------------------------------------------
// ⿨ Configure Routing
// --------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// --------------------------------------------------------
// ⿩ Run the Application
// --------------------------------------------------------
app.Run();