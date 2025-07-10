[![NuGet](https://img.shields.io/nuget/v/Dom.Mediator.svg)](https://www.nuget.org/packages/Dom.Mediator)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](../LICENSE)

# Dom.Mediator

**Dom.Mediator** is a clean, minimal, zero-dependency implementation of the Mediator pattern for .NET, focused on:

- ✅ Request/Response messaging (CQRS)
- ✅ Composable pipeline behaviors (validation, logging, etc.)
- ✅ Built-in `Result<T>` handling with typed errors
- ✅ No external dependencies — just C#
- ✅ Perfect fit for Minimal APIs or Clean Architecture

## 📦 Install via NuGet

```bash
dotnet add package Dom.Mediator
```

## 🚀 Quick Example

Define your command:

```csharp
public class EchoCommand : IRequest<string>
{
    public string Message { get; set; } = string.Empty;
}
```

Create a handler:

```csharp
public class EchoHandler : IRequestHandler<EchoCommand, string>
{
    public Task<Result<string>> Handle(EchoCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<string>.Success($"Echo: {request.Message}"));
    }
}
```

Register and send:

```csharp
var mediator = new Dom.Mediator.PureMediator();
mediator.ScanHandlersFrom(Assembly.GetExecutingAssembly());

var result = await mediator.Send(new EchoCommand { Message = "Hello" });

return result.Match(
    value => Results.Ok(value),
    error => Results.Problem(error)
);
```

## ⚙️ Built-in Features

- 🧱 Pipeline Behaviors: Add logging, validation, error handling in a clean chain.
- 🎯 Result Pattern (Result<T> and Result): Native success/failure with structured error codes.
- 🧪 FluentValidation Support: Easily add a validation behavior.
- 🔄 Auto-Handler Discovery: Reflective scanning via assembly input.
- 💡 Custom Behaviors: Plug in your own pre-/post-handling logic.

## 🧩 Extensibility

```csharp
mediator.RegisterGlobalPipelineBehavior(typeof(MyCustomBehavior<,>));
```

Each behavior implements:

```csharp
Task<Result<TResponse>> Handle(
    TRequest request,
    CancellationToken cancellationToken,
    RequestHandlerDelegate<TResponse> next);
```

## 📜 License

Dom.Mediator is [MIT licensed](LICENSE).

## 👤 Author

Made with ❤️ by [André Dominic Santacroce](https://www.andresantacroce.com)
