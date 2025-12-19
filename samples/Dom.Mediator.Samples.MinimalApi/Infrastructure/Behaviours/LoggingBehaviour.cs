using Dom.Mediator.Abstractions;
using System.Text.Json;

namespace Dom.Mediator.Samples.MinimalApi.Infrastructure.Behaviours;

/// <summary>
/// Unified logging behaviour that works with both request/response and command-only patterns
/// </summary>
public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        try
        {
            _logger.LogInformation(
                "[MDTR] - Handling request of type {RequestType}:\r\n\r\n{RequestData}\r\n",
                typeof(TRequest).Name,
                JsonSerializer.Serialize(request));

            var response = await next();

            if (response.IsSuccess)
            {
                _logger.LogInformation(
                    "[MDTR] - Response generated for request of type {RequestType}:\r\n\r\n{Response}\r\n",
                    typeof(TRequest).Name,
                    JsonSerializer.Serialize(response.Value));
            }
            else
            {
                _logger.LogError(
                    "[MDTR] - Response generated with error for request of type {RequestType}:\r\n\r\n{Response}",
                    typeof(TRequest).Name,
                    JsonSerializer.Serialize(response));
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MDTR: Error");

            return Result<TResponse>.Failure(new Error(ex.GetType().ToString(), ex.Message, "Internal"));
        }
    }
}

/// <summary>
/// Unified logging behaviour for commands (no response)
/// This allows the same LoggingBehaviour to work with both patterns
/// </summary>
public class LoggingBehaviour<TRequest> : IPipelineBehavior<TRequest>
    where TRequest : ICommand
{
    private readonly ILogger<LoggingBehaviour<TRequest>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(TRequest request, CancellationToken cancellationToken, CommandHandlerDelegate next)
    {
        try
        {
            _logger.LogInformation(
                "[MDTR] - Handling command of type {RequestType}:\r\n\r\n{RequestData}\r\n",
                typeof(TRequest).Name,
                JsonSerializer.Serialize(request));

            var response = await next();

            if (response.IsSuccess)
            {
                _logger.LogInformation(
                    "[MDTR] - Command completed for type {RequestType}:\r\n\r\n{Response}\r\n",
                    typeof(TRequest).Name,
                    JsonSerializer.Serialize(response));
            }
            else
            {
                _logger.LogError(
                    "[MDTR] - Command failed for type {RequestType}:\r\n\r\n{Response}\r\n",
                    typeof(TRequest).Name,
                    JsonSerializer.Serialize(response));
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MDTR: Error");

            return Result.Failure(new Error(ex.GetType().ToString(), ex.Message, "Internal"));
        }
    }
}