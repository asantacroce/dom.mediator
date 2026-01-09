using Dom.Mediator.Abstractions;
using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace Dom.Mediator.Samples.MinimalApi.Infrastructure.Behaviours;

/// <summary>
/// FluentValidation implementation of the AbstractValidationBehaviour
/// </summary>
public class FluentValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
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
            .Where(f => f != null)
            .ToList();
        
        if (failures.Count == 0)
        {
            return await next();
        }

        Error error = new Error("ValidationError", "One or more validation errors occurred.", "Validation");
        foreach (var failure in failures)
        {
            error.AddDetail(failure.ErrorCode, failure.ErrorMessage);
        }

        return Result<TResponse>.Failure(error);
    }
}

public class FluentValidationBehaviour<TRequest> : IPipelineBehavior<TRequest>
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
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        Error error = new Error("ValidationError", "One or more validation errors occurred.", "Validation");
        foreach (var failure in failures)
        {
            error.AddDetail(failure.ErrorCode, failure.ErrorMessage);
        }

        return Result.Failure(error);
    }
}