using DanceSchool.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DanceSchool.Extensions;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<DanceSchoolDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
        );

        services.AddHealthChecks()
            .AddNpgSql(connectionString!);

        return services;
    }
}
