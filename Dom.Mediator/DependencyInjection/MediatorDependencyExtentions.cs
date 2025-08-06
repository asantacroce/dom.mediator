using Dom.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dom.Mediator;

public static class MediatorDependencyExtentions
{
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<IMediator> config)
    {
        // We register the IMediator interface and its implementation
        services.AddSingleton<IMediator>(serviceProvider => { 
            Implementation.Mediator mediator = new (serviceProvider);

            // Apply any configuration passed in
            config(mediator);

            return mediator;
        });

        return services;
    }
}
