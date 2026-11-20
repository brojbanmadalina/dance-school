namespace DanceSchool.Extensions;

public static class SignalRExtensions
{
    public const string ChatCorsPolicy = "ChatTest";

    public static IServiceCollection AddSignalRChat(this IServiceCollection services)
    {
        services.AddSignalR();

        services.AddCors(options =>
        {
            options.AddPolicy(ChatCorsPolicy, policy =>
                policy.SetIsOriginAllowed(_ => true)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials());
        });

        return services;
    }
}
