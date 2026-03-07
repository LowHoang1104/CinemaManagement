using CinemaManagement;
using CinemaManagement.Data;
using CinemaManagement.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<CinemaManagementContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MyCnn"))
);

builder.Services.AddApplicationServices();
builder.Services.AddSignalR();

// Đăng ký SeatNotifier để inject vào BookingService
builder.Services.AddScoped<ISeatNotifier, SeatNotifier>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=AdminUsers}/{action=Index}/{id?}");

// Map SignalR Hub
app.MapHub<SeatHub>("/hubs/seat");

app.Run();
