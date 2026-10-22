using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.Business.Jobs;
using DanceSchool.Business.Models.Auth;
using DanceSchool.Business.Services.Auth;
using DanceSchool.Api.Services;

namespace DanceSchool.Api.Extensions;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.Configure<EmailSettings>(configuration.GetSection("AuthSettings:EmailSettings"));
        services.Configure<LockoutSettings>(configuration.GetSection("AuthSettings:LockoutSettings"));
        services.Configure<ForgotPasswordSettings>(configuration.GetSection("AuthSettings:ForgotPasswordSettings"));

        services.AddScoped<EmailTemplateService>(_ => new EmailTemplateService(
            Path.Combine(environment.ContentRootPath, "Templates")
        ));

        services.AddHttpContextAccessor();
        services.AddScoped<IUserProvider, UserProvider>();
        services.AddScoped<CourseReminderJob>();

        return services;
    }
}
