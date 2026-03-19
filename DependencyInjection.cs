using CinemaManagement.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CinemaManagement
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IAdminUserService, AdminUserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IExportService, ExportService>();   
            services.AddScoped<ICinemaService, CinemaService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IShowTimeService, ShowTimeService>();

            return services;
        }
    }
}
