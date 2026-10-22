using DanceSchool.Business.Interfaces.Attendances;
using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.Business.Interfaces.Chat;
using DanceSchool.Business.Interfaces.Common;
using DanceSchool.Business.Interfaces.Courses;
using DanceSchool.Business.Interfaces.Profile;
using DanceSchool.Business.Services.Attendances;
using DanceSchool.Business.Services.Auth;
using DanceSchool.Business.Services.Chat;
using DanceSchool.Business.Services.Common;
using DanceSchool.Business.Services.Courses;
using DanceSchool.Business.Services.DanceStyle;
using DanceSchool.Business.Services.UserProfile;
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
        services.AddScoped<IAttendanceService, AttendanceService>();

        // Other
        services.AddScoped<DanceStyleService>();

        //User profile
        services.AddScoped<IProfileService, UserProfileService>();

        return services;
    }
}
