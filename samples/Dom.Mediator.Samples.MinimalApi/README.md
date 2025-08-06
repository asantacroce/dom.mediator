# 🚀 Dom.Mediator Minimal API Sample

This sample project demonstrates how to use the **Dom.Mediator** library to implement the CQRS (Command Query Responsibility Segregation) pattern in an ASP.NET Core Minimal API application.

## 📋 Overview

This sample showcases a simple Task Management API that implements:

- **CQRS Pattern** with separate Commands and Queries
- **Mediator Pattern** for decoupled request handling
- **Pipeline Behaviors** for cross-cutting concerns (logging)
- **Result Pattern** for error handling
- **Minimal API** endpoints with Swagger documentation

## ✨ Features

### 📝 Commands
- **CreateTaskCommand**: Creates a new task with validation

### 🔍 Queries
- **GetAllTasksQuery**: Retrieves all tasks from the in-memory store

### 🔧 Pipeline Behaviors
- **LoggingBehaviour**: Logs all request/response operations for queries
- **LoggingCommandBehaviour**: Specialized logging for command operations

### 🏗️ Infrastructure
- **TaskStore**: In-memory task storage
- **HttpResult**: Custom result types for HTTP responses
- **Validation**: Built-in validation with detailed error reporting

## 📁 Project Structure

```
├── Features/
│   ├── CreateTask/
│   │   ├── CreateTaskCommand.cs       # Command definition
│   │   └── CreateTaskHandler.cs       # Command handler with validation
│   └── GetAllTasks/
│       ├── GetAllTasksQuery.cs        # Query definition
│       └── GetAllTasksHandler.cs      # Query handler
├── Infrastructure/
│   ├── Behaviours/
│   │   ├── LoggingBehaviour.cs        # Request/Response logging
│   │   └── LoggingCommandBehaviour.cs # Command-specific logging
│   ├── Endpoints/
│   │   └── Endpoints.cs               # API endpoint definitions
│   ├── Repositories/
│   │   └── TaskStore.cs               # In-memory data store
│   └── Results/
│       ├── HttpResult.cs              # HTTP result wrappers
│       └── ResultExtension.cs         # Extension methods
└── Program.cs                         # Application configuration
```

## 🚀 Getting Started

### 📋 Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

### ▶️ Running the Application

1. Clone the repository and navigate to the sample directory:
   ```bash
   cd Dom.Mediator.Samples.MinimalApi
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

4. The application will start and be available at:
   - HTTP: `http://localhost:5130`
   - HTTPS: `https://localhost:7003`
   - Swagger UI: `https://localhost:7003/swagger`


## 🔑 Key Implementation Details

### ⚙️ Mediator Configuration

The mediator is configured in `Program.cs` with automatic handler registration:

```csharp
builder.Services.AddMediator(config =>
{
    // Register command/query handlers
    config.RegisterHandlers(typeof(Program).Assembly);

    // Register pipeline behaviors
    config.AddRequestResponseBehaviour(typeof(LoggingBehaviour<,>));
    config.AddCommandBehaviour(typeof(LoggingCommandBehaviour<>));
});
```

### ✅ Command Handler with Validation

The `CreateTaskHandler` demonstrates validation and error handling:

```csharp
public Task<Result> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
{
    if(Validate(request) is Error error)
    {
        return Task.FromResult(Result.Failure(error));
    }
    
    // Create and store task...
    return Task.FromResult(Result.Success());
}
```

### 🔄 Pipeline Behaviors

The logging behaviors provide automatic request/response logging:

- `LoggingBehaviour<TRequest, TResponse>`: For queries with responses
- `LoggingCommandBehaviour<TRequest>`: For commands without return values

### 📊 Result Pattern

The application uses a Result pattern for error handling:

- `Result`: For operations without return values
- `Result<T>`: For operations with return values
- Automatic HTTP status code mapping based on error types

## 🎓 What You'll Learn

By exploring this sample, you'll understand:

1. 🔧 **How to set up Dom.Mediator** in a Minimal API application
2. 🏛️ **CQRS implementation** with separate commands and queries
3. 🔗 **Pipeline behaviors** for cross-cutting concerns
4. ✅ **Validation patterns** with detailed error reporting
5. 📈 **Result pattern** for consistent error handling
6. 🌐 **HTTP integration** with proper status codes and responses

## 🛠️ Technologies Used

- 🟣 **.NET 8.0**
- 🌐 **ASP.NET Core Minimal APIs**
- 🎯 **Dom.Mediator** - CQRS/Mediator implementation
- 📚 **Swagger/OpenAPI** - API documentation
- 📝 **System.Text.Json** - JSON serialization

## 🚀 Next Steps

This sample provides a foundation for building more complex applications.

Consider extending it with:

- 🗃️ Database integration (Entity Framework Core)
- 🔐 Authentication and authorization
- ✅ More complex validation rules
- 📊 Additional pipeline behaviors (caching, performance monitoring)
- 🧪 Unit and integration tests
- ➕ Additional CRUD operations (Update, Delete tasks)
