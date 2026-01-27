using FluentValidation;
using System.Reflection;

namespace Dom.Mediator.Samples.MinimalApi.Infrastructure.Behaviours;

public static class FluentValidationServiceExtentions
{
    public static IServiceCollection AddValidatorsFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var validatorType = typeof(IValidator<>);

        var validatorTypes = assembly.GetExportedTypes()
            .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
            .Select(t => new { Type = t, Interfaces = t.GetInterfaces() })
            .Where(t => t.Interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == validatorType))
            .ToList();

        foreach (var validator in validatorTypes)
        {
            var interfaceType = validator.Interfaces.First(i => i.IsGenericType && i.GetGenericTypeDefinition() == validatorType);
            var validatedType = interfaceType.GetGenericArguments()[0];
            var serviceType = validatorType.MakeGenericType(validatedType);

            services.AddTransient(serviceType, validator.Type);
        }

        return services;
    }
}