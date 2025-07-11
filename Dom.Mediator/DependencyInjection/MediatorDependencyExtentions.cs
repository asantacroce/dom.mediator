using Microsoft.Extensions.DependencyInjection;

namespace Dom.Mediator.DependencyInjection;

public static class MediatorDependencyExtentions
{
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<IMediator> config)
    {
        // We register the IMediator interface and its implementation
        services.AddSingleton<IMediator>(serviceProvider => { 
            Mediator mediator = new Mediator(serviceProvider);

            // Apply any configuration passed in
            config(mediator);

            return mediator;
        });

        return services;
    }
}
