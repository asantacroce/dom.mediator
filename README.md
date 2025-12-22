[![NuGet](https://img.shields.io/nuget/v/Dom.Mediator.svg)](https://www.nuget.org/packages/Dom.Mediator)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](../LICENSE)

# Dom.Mediator

**Dom.Mediator** is a clean, minimal implementation of the Mediator pattern for .NET, focused on:

- ✅ Request/Response messaging (CQRS)
- ✅ Composable pipeline behaviors (validation, logging, etc.)
- ✅ Built-in Result Pattern handling with typed errors
- ✅ Lightweight with minimal dependencies
- ✅ Perfect fit for Minimal APIs or Clean Architecture

## 📦 Install via NuGet

```bash
dotnet add package Dom.Mediator
```

## 🚀 Quick Example

### Commands (Actions without return values)

Define your command:
```csharp
public record CreateTaskCommand(string Title, string? Description) : ICommand;
```

Create a handler:
```csharp
public class CreateTaskHandler : ICommandHandler<CreateTaskCommand>
{
    public Task<Result> Handle(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        // Perform action logic here
        return Task.FromResult(Result.Success());
    }
}
```

### Queries (Read operations with return values)

Define your query:
```csharp
public record GetTasksQuery() : IQuery<List<TaskItem>>;
```

Create a handler:
```csharp
public class GetTasksHandler : IQueryHandler<GetTasksQuery, List<TaskItem>>
{
    public Task<Result<List<TaskItem>>> Handle(GetTasksQuery query, CancellationToken cancellationToken)
    {
        var tasks = GetTasksFromStore(); // Your logic here
        return Task.FromResult(Result<List<TaskItem>>.Success(tasks));
    }
}
```

### Setup with Dependency Injection

```csharp
builder.Services.AddMediator(config =>
{
    config.RegisterHandlers(typeof(Program).Assembly);
    
    // Add pipeline behaviors - automatically handles both queries and commands
    config.AddBehaviour(typeof(LoggingBehaviour<,>));  // For queries (request/response)
    config.AddBehaviour(typeof(LoggingBehaviour<>));   // For commands (no response)
});
```

## 📚 Samples & Tests

Check out the [**samples**](https://github.com/asantacroce/dom.mediator/tree/main/samples) directory for complete working examples:

> 🧪 **Testing**: See the [**tests**](https://github.com/asantacroce/dom.mediator/tree/main/tests) directory for unit tests, coverage reports, and testing documentation.

> 📊 **Coverage**: Here also the [**coverage report**](https://mediator.andresantacroce.com/coverage/) related to the latest version.

### 🌐 [Minimal API Sample](https://github.com/asantacroce/dom.mediator/tree/main/samples/Dom.Mediator.Samples.MinimalApi)
A complete ASP.NET Core Minimal API implementation demonstrating:
- **Task Management API** with CQRS pattern
- **Command & Query handlers** with validation
- **Pipeline behaviors** for logging and cross-cutting concerns
- **Result pattern** integration with HTTP responses
- **Swagger documentation** and endpoint configuration

```bash
cd samples/Dom.Mediator.Samples.MinimalApi
dotnet run
```

## ⚙️ Built-in Features

- 🧱 **Pipeline Behaviors**: Add logging, validation, error handling in a clean chain
- 🎯 **Result Pattern**: Native success/failure with structured error codes (`Result<T>` and `Result`)
- 🔄 **Auto-Handler Discovery**: Reflective scanning via assembly registration
- 💡 **CQRS Support**: Separate Command and Query handling with distinct interfaces
- 🏗️ **Dependency Injection**: Built-in support for Microsoft DI container

## 🧩 Pipeline Behaviors

Add behaviors to intercept and process requests using a unified registration method:

```csharp
// Single registration method for both query and command behaviors
config.AddBehaviour(typeof(LoggingBehaviour<,>));      // For queries (2 generic parameters)
config.AddBehaviour(typeof(ValidationBehaviour<>));    // For commands (1 generic parameter)
```

The mediator automatically detects whether your behavior is for:
- **Queries** (request/response) - behaviors with 2 generic type parameters
- **Commands** (no response) - behaviors with 1 generic type parameter

### Query Behavior Interface (Request/Response):
```csharp
public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<Result<TResponse>> Handle(
        TRequest request, 
        CancellationToken cancellationToken, 
        RequestHandlerDelegate<TResponse> next);
}
```

### Command Behavior Interface (No Response):
```csharp
public interface IPipelineBehavior<TCommand>
    where TCommand : ICommand
{
    Task<Result> Handle(
        TCommand command, 
        CancellationToken cancellationToken, 
        CommandHandlerDelegate next);
}
```

### Example Behavior Implementation:
```csharp
// Query behavior (2 generic parameters)
public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
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
        _logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);
        var response = await next();
        _logger.LogInformation("Handled {RequestType}", typeof(TRequest).Name);
        return response;
    }
}

// Command behavior (1 generic parameter)
public class LoggingBehaviour<TCommand> : IPipelineBehavior<TCommand>
    where TCommand : ICommand
{
    private readonly ILogger<LoggingBehaviour<TCommand>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TCommand>> logger)
        => _logger = logger;

    public async Task<Result> Handle(
        TCommand command,
        CancellationToken cancellationToken,
        CommandHandlerDelegate next)
    {
        _logger.LogInformation("Handling {CommandType}", typeof(TCommand).Name);
        var response = await next();
        _logger.LogInformation("Handled {CommandType}", typeof(TCommand).Name);
        return response;
    }
}
```

## 📜 License

Dom.Mediator is [MIT licensed](LICENSE).

## 👤 Author

Made with ❤️ by [André Dominic Santacroce](https://www.andresantacroce.com)