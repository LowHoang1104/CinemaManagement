using CinemaManagement;
using CinemaManagement.Data;
using CinemaManagement.Models;
using CinemaManagement.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using CinemaManagement.Hubs;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authentication.Google;


var builder = WebApplication.CreateBuilder(args);

// Set UTF-8 encoding for Vietnamese support
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
Console.OutputEncoding = Encoding.UTF8;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

builder.Services.AddDbContext<CinemaManagementContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MyCnn"))
);

builder.Services.AddApplicationServices();
builder.Services.AddSignalR();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["GoogleAuth:ClientId"];
    options.ClientSecret = builder.Configuration["GoogleAuth:ClientSecret"];
    options.CallbackPath = "/signin-google";

    options.Scope.Add("profile");
    options.Scope.Add("email");
});


builder.Services.AddAuthorization();

// Session đăng nhập 30p
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Đăng ký SeatNotifier từ Services namespace để inject vào BookingController
builder.Services.AddScoped<CinemaManagement.Services.ISeatNotifier, CinemaManagement.Services.SeatNotifier>();

// Đăng ký CoupleSeatService
builder.Services.AddScoped<ICoupleSeatService, CoupleSeatService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// Map SignalR Hub
app.MapHub<SeatHub>("/hubs/seat");

// Map SignalR hub
app.MapHub<SeatHub>("/seatHub");

app.Run();

