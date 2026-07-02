using DanceSchool.Business.Configurations;
using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.Business.Interfaces.Chat;
using DanceSchool.Business.Interfaces.Common;
using DanceSchool.Business.Interfaces.Courses;
using DanceSchool.Business.Jobs;
using DanceSchool.Business.Models.Auth;
using DanceSchool.Business.Services.Auth;
using DanceSchool.Business.Services.Chat;
using DanceSchool.Business.Services.Common;
using DanceSchool.Business.Services.Courses;
using DanceSchool.Business.Services.DanceStyle;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DanceSchool.Business.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddJwt(this IServiceCollection services)
    {
        services.ConfigureOptions<JwtOptionsSetup>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.Key)),
                    ClockSkew = TimeSpan.Zero
                };
                //chat
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
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

        // Other
        services.AddScoped<DanceStyleService>();
        services.AddScoped<CourseReminderJob>();

        // Infrastructure
        services.AddHttpContextAccessor();

        // Config
        services.Configure<EmailSettings>(configuration.GetSection("AuthSettings:EmailSettings"));
        services.Configure<LockoutSettings>(configuration.GetSection("AuthSettings:LockoutSettings"));
        services.Configure<ForgotPasswordSettings>(configuration.GetSection("AuthSettings:ForgotPasswordSettings"));

        services.AddScoped<EmailTemplateService>(_ => new EmailTemplateService(
            Path.Combine(environment.ContentRootPath, "Templates")
        ));

        return services;
    }
}
