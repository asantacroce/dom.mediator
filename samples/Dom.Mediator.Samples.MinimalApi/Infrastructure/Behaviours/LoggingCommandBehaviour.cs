using Dom.Mediator.Abstractions;
using System.Text.Json;

namespace Dom.Mediator.Samples.MinimalApi.Infrastructure.Behaviours;

public class LoggingCommandBehaviour<TRequest> : IPipelineBehavior<TRequest>
    where TRequest : ICommand
{
    private readonly ILogger<LoggingCommandBehaviour<TRequest>> _logger;

    public LoggingCommandBehaviour(ILogger<LoggingCommandBehaviour<TRequest>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(TRequest request, CancellationToken cancellationToken, CommandHandlerDelegate next)
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
                    JsonSerializer.Serialize(response));
            }
            else
            {
                _logger.LogError(
                    "[MDTR] - Response generated with error for request of type {RequestType}:\r\n\r\n{Response}\r\n",
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
