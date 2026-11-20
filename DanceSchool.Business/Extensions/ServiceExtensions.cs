using DanceSchool.Business.Interfaces;
using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.Business.Interfaces.Common;
using DanceSchool.Business.Interfaces.Groups;
using DanceSchool.Business.Services.Auth;
using DanceSchool.Business.Services;
using DanceSchool.Business.Services.Groups;
using Microsoft.Extensions.DependencyInjection;

namespace DanceSchool.Business.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        // Auth
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ILockoutService, LockoutService>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IPasswordResetTokenService, PasswordResetTokenService>();

        // Courses
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IGroupScheduleService, GroupScheduleService>();
        services.AddScoped<ILocationService, LocationService>();

        //Chat
        services.AddScoped<IChatService, ChatService>();

        //Attendance


        // Other
        services.AddScoped<DanceStyleService>();

        //User profile
        services.AddScoped<IProfileService, UserProfileService>();

        return services;
    }
}
