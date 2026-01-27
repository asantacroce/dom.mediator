using Dom.Mediator.Abstractions;
using FluentValidation;

namespace Dom.Mediator.Samples.MinimalApi.Infrastructure.Behaviours;

/// <summary>
/// FluentValidation implementation of the AbstractValidationBehaviour
/// </summary>
public class FluentValidationBehaviour<TRequest, TResponse> : IPipelineBehaviour<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public FluentValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null);

        if (failures.Count() > 0)
        {
            Error error = new("ValidationError", "One or more validation errors occurred.", "Validation");

            foreach (var failure in failures)
            {
                error.AddDetail(failure.PropertyName, $"{failure.ErrorCode}:{failure.ErrorMessage}");
            }

            return Result<TResponse>.Failure(error);
        }
        else
        {
            return await next();
        }
    }
}

public class FluentValidationBehaviour<TRequest> : IPipelineBehaviour<TRequest>
    where TRequest : ICommand
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public FluentValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<Result> Handle(TRequest request, CancellationToken cancellationToken, CommandHandlerDelegate next)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null);

        if (failures.Count() > 0)
        {
            Error error = new("ValidationError", "One or more validation errors occurred.", "Validation");

            foreach (var failure in failures)
            {
                error.AddDetail(failure.PropertyName, $"{failure.ErrorCode}:{failure.ErrorMessage}");
            }

            return Result.Failure(error);
        }
        else
        {
            return await next();
        }   
    }
}