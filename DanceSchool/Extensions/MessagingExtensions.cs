using DanceSchool.Business.Interfaces.Common;
using DanceSchool.Business.Jobs;
using DanceSchool.Consumers;
using DanceSchool.Services;
using MassTransit;
using Quartz;

namespace DanceSchool.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqConnectionString = configuration.GetConnectionString("rabbitmq")
            ?? throw new InvalidOperationException("Connection string 'rabbitmq' was not found.");

        services.AddMassTransit(x =>
        {
            x.AddConsumer<CourseCreatedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqConnectionString);
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        return services;
    }

    public static IServiceCollection AddScheduling(this IServiceCollection services, IConfiguration configuration)
    {
        var defaultConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddQuartz(q =>
        {
            q.UsePersistentStore(store =>
            {
                store.UseProperties = true;
                store.UsePostgres(pg =>
                {
                    pg.ConnectionString = defaultConnectionString;
                    pg.TablePrefix = "quartz.qrtz_";
                });
                store.UseSystemTextJsonSerializer();
            });

            var jobKey = new JobKey("CourseReminderJob");
            q.AddJob<CourseReminderJob>(opts => opts.WithIdentity(jobKey).StoreDurably());
            q.AddTrigger(opts =>
                opts.ForJob(jobKey)
                    .WithIdentity("CourseReminderTrigger")
                    .WithCronSchedule(
                        "0 0 8 * * ?",
                        x =>
                        {
                            x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Europe/Bucharest"));
                            x.WithMisfireHandlingInstructionFireAndProceed();
                        }
                    )
            );
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        services.AddQuartzDashboard();

        return services;
    }
}
