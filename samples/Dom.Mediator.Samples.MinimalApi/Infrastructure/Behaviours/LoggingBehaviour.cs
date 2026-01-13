using Dom.Mediator.Abstractions;
using System.Text.Json;

namespace Dom.Mediator.Samples.MinimalApi.Infrastructure.Behaviours;

/// <summary>
/// Shared logging logic for all behaviors
/// </summary>
internal static class LoggingHelper
{
    public static void LogRequest(ILogger logger, object request)
    {
        logger.LogInformation(
            "[MDTR] - Handling {RequestType}:\r\n{Data}\r\n",
            request.GetType().Name,
            JsonSerializer.Serialize(request));
    }

    public static void LogResult(ILogger logger, Type requestType, Result result)
    {
        if (result.IsSuccess)
        {
            LogSuccess(logger, requestType, result);
        }
        else
        {
            LogFailure(logger, requestType, result);
        }   
    }

    public static void LogResult<T>(ILogger logger, Type requestType, Result<T> result)
    {
        if (result.IsSuccess)
        {
            LogSuccess(logger, requestType, result);
        }
        else
        {
            LogFailure(logger, requestType, result);
        }
    }

    public static Error CreateErrorFromException(ILogger logger, Exception ex)
    {
        logger.LogError(ex, "[MDTR] - Exception occurred");
        return new Error(ex.GetType().ToString(), ex.Message, "Internal");
    }

    private static void LogSuccess(ILogger logger, Type requestType, object? result = null)
    {
        logger.LogInformation(
            "[MDTR] - ✓ Completed {RequestType}{Response}\r\n",
            requestType.Name,
            result != null ? $":\r\n{JsonSerializer.Serialize(result)}" : "");
    }

    private static void LogFailure(ILogger logger, Type requestType, object result)
    {
        logger.LogError(
            "[MDTR] - ✗ Failed {RequestType}:\r\n{Errors}\r\n",
            requestType.Name,
            JsonSerializer.Serialize(result));
    }
}

/// <summary>
/// Logging behavior for queries (request/response)
/// </summary>
public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehaviour<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<Result<TResponse>> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        try
        {
            LoggingHelper.LogRequest(_logger, request);
            var response = await next();
            LoggingHelper.LogResult(_logger, typeof(TRequest), response);

            return response;
        }
        catch (Exception ex)
        {
            return Result<TResponse>.Failure(
                LoggingHelper.CreateErrorFromException(_logger, ex));
        }
    }
}

/// <summary>
/// Logging behavior for commands (no response)
/// </summary>
public class LoggingBehaviour<TRequest> : IPipelineBehaviour<TRequest>
    where TRequest : ICommand
{
    private readonly ILogger<LoggingBehaviour<TRequest>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest>> logger)
        => _logger = logger;

    public async Task<Result> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        CommandHandlerDelegate next)
    {
        try
        {
            LoggingHelper.LogRequest(_logger, request);
            var response = await next();
            LoggingHelper.LogResult(_logger, typeof(TRequest), response);

            return response;
        }
        catch (Exception ex)
        {
            return Result.Failure(
                LoggingHelper.CreateErrorFromException(_logger, ex));
        }
    }
}