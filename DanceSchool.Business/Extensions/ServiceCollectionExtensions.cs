using Microsoft.Extensions.DependencyInjection;

namespace DanceSchool.Business.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices<TMarker>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var assembly = typeof(TMarker).Assembly;

        var concreteTypes = assembly.GetTypes()
            .Where(type =>
                type.IsClass
                && !type.IsAbstract //abstract+static
                && !type.IsGenericTypeDefinition
                && !type.IsNested);

        foreach (var implementationType in concreteTypes)
        {
            var interfaceType = implementationType
                .GetInterfaces()
                .FirstOrDefault(i => i.Assembly == assembly);

            if (interfaceType == null)
                continue;

            if (!interfaceType.IsAssignableFrom(implementationType))
                continue;

            var descriptor = new ServiceDescriptor(interfaceType, implementationType, lifetime);
            services.Add(descriptor);
        }

        return services;
    }
}
