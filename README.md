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
    
    // Add pipeline behaviors
    config.AddRequestResponseBehaviour(typeof(LoggingBehaviour<,>));
    config.AddCommandBehaviour(typeof(ValidationBehaviour<>));
});
);
```

## ⚙️ Built-in Features

- 🧱 **Pipeline Behaviors**: Add logging, validation, error handling in a clean chain
- 🎯 **Result Pattern**: Native success/failure with structured error codes (`Result<T>` and `Result`)
- 🔄 **Auto-Handler Discovery**: Reflective scanning via assembly registration
- 💡 **CQRS Support**: Separate Command and Query handling with distinct interfaces
- 🏗️ **Dependency Injection**: Built-in support for Microsoft DI container

## 🧩 Pipeline Behaviors

Add behaviors to intercept and process requests:

```csharp
// For queries (request/response)
config.AddRequestResponseBehaviour(typeof(LoggingBehaviour<,>));

// For commands
config.AddCommandBehaviour(typeof(ValidationBehaviour<>));
```

### Request/Response Behavior Interface:
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

### Command Behavior Interface:
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

## 📜 License

Dom.Mediator is [MIT licensed](LICENSE).

## 👤 Author

Made with ❤️ by [André Dominic Santacroce](https://www.andresantacroce.com)