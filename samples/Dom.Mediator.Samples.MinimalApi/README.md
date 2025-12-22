# 🚀 Dom.Mediator Minimal API Sample

This sample project demonstrates how to use the **Dom.Mediator** library to implement the CQRS (Command Query Responsibility Segregation) pattern in an ASP.NET Core Minimal API application with **Domain-Driven Design** principles.

## 📋 Overview

This sample showcases a **Task Management API** that implements:

- ✅ **CQRS Pattern** with separate Commands and Queries
- ✅ **Mediator Pattern** for decoupled request handling
- ✅ **Unified Pipeline Behaviors** for cross-cutting concerns (logging)
- ✅ **Result Pattern** for structured error handling
- ✅ **Rich Domain Model** with encapsulated business rules
- ✅ **Domain-Driven Design** principles
- ✅ **Minimal API** endpoints with Swagger documentation

## ✨ Features

### 📝 Commands
- **CreateTaskCommand**: Creates a new task with validation
- **UpdateTaskCommand**: Updates task status with state transition validation

### 🔍 Queries
- **GetAllTasksQuery**: Retrieves all tasks from the in-memory store

### 🏗️ Domain Model
- **TaskItem**: Rich domain model that enforces business invariants
  - Status transition rules (Created → InProgress → ReadyToTest → Completed)
  - State history tracking
  - Comment management
  - Due date validation

### 🔧 Pipeline Behaviors
- **Unified LoggingBehaviour**: Single behavior implementation that works with both:
  - `LoggingBehaviour<TRequest, TResponse>`: For queries with return values
  - `LoggingBehaviour<TRequest>`: For commands without return values
  - Shared logging logic via `LoggingHelper`

### 🏗️ Infrastructure
- **TaskRepository**: In-memory task storage
- **HttpResult**: Custom result types for HTTP responses
- **Rich validation**: Domain-level validation with detailed error reporting

## 📁 Project Structure

```
├── Features/
│   ├── TaskItem.cs                    # Rich domain model with business rules
│   ├── CreateTask/
│   │   ├── CreateTaskCommand.cs       # Command definition
│   │   └── CreateTaskHandler.cs       # Command handler with validation
│   ├── UpdateTask/
│   │   ├── UpdateTaskCommand.cs       # Update command definition
│   │   └── UpdateTaskHandler.cs       # Update handler with domain orchestration
│   └── GetAllTasks/
│       ├── GetAllTasksQuery.cs        # Query definition
│       └── GetAllTasksHandler.cs      # Query handler
├── Infrastructure/
│   ├── Behaviours/
│   │   └── LoggingBehaviour.cs        # Unified logging behavior (2 arities)
│   ├── Endpoints/
│   │   └── Endpoints.cs               # API endpoint definitions
│   ├── Repositories/
│   │   └── TaskRepository.cs          # In-memory data store
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

## 📡 API Endpoints

### Create Task
```http
POST /tasks
Content-Type: application/json

{
  "title": "Implement new feature",
  "description": "Add user authentication",
  "dueDate": "2024-12-31T23:59:59Z"
}
```

### Get All Tasks
```http
GET /tasks
```

### Update Task
```http
PUT /tasks
Content-Type: application/json

{
  "id": "task-id-here",
  "comment": "Moving to testing phase",
  "status": "ReadyToTest"
}
```

**Note:** Status enum supports both string and numeric values:
- String: `"Created"`, `"InProgress"`, `"ReadyToTest"`, `"Completed"`
- Numeric: `0`, `1`, `2`, `3`

## 🔑 Key Implementation Details

### ⚙️ Mediator Configuration

The mediator is configured in `Program.cs` with automatic handler registration and **unified behaviors**:

```csharp
builder.Services.AddMediator(config =>
{
    // Register command/query handlers
    config.RegisterHandlers(typeof(Program).Assembly);

    // Register unified logging behaviors (both arities)
    config.AddBehaviour(typeof(LoggingBehaviour<,>));  // For queries/requests
    config.AddBehaviour(typeof(LoggingBehaviour<>));   // For commands
});
```

### 🎯 Unified Pipeline Behavior

The **unified LoggingBehaviour** eliminates code duplication by:
1. Having **two class definitions** with different generic arities in the same file
2. Sharing common logging logic via `LoggingHelper`
3. Automatically working with both commands and queries

```csharp
// For queries (request/response)
public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{ /* ... */ }

// For commands (no response)
public class LoggingBehaviour<TRequest> : IPipelineBehavior<TRequest>
    where TRequest : ICommand
{ /* ... */ }
```

### 🏛️ Rich Domain Model

The `TaskItem` class demonstrates **Domain-Driven Design** by:
- **Encapsulating business rules** (status transitions, validation)
- **Protecting invariants** (private setters, factory method)
- **Self-validation** (business logic in the domain)

```csharp
// Factory method with validation
public static Result<TaskItem> Create(string title, string description, DateTime? dueDate)
{
    // Domain validation
    if (dueDate.HasValue && dueDate < DateTime.UtcNow)
        return Result<TaskItem>.Failure(/* ... */);
    
    var task = new TaskItem { /* ... */ };
    return Result<TaskItem>.Success(task);
}

// Business rule enforcement
public Result ChangeStatus(Status newStatus, string comment)
{
    // Enforce status transition rules
    if (CurrentStatus == Status.Completed)
        return Result.Failure(/* Cannot update completed tasks */);
    
    if (NotTested() && newStatus == Status.Completed)
        return Result.Failure(/* Must be tested first */);
    
    // Valid transition
    CurrentStatus = newStatus;
    TrackStatus(CurrentStatus);
    return Result.Success();
}
```

### ✅ Command Handler Orchestration

Handlers are now **thin orchestration layers** that delegate to the domain model:

```csharp
public async Task<Result> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
{
    // Find the task
    var task = _store.Tasks.SingleOrDefault(x => x.Id == request.Id);
    if (task is null)
        return Result.Failure(/* task not found */);

    // Delegate status change to domain model
    if (request.Status.HasValue)
    {
        var statusChange = task.ChangeStatus(request.Status.Value, request.Comment);
        if (statusChange.IsFailure)
            return statusChange;
    }

    // Add comment
    task.AddComment(request.Comment);
    return Result.Success();
}
```

### 📊 Result Pattern

The application uses a comprehensive Result pattern:

- **`Result`**: For operations without return values
- **`Result<T>`**: For operations with return values
- **Structured errors** with codes, descriptions, and types
- **Error details** for validation failures
- **Automatic HTTP status code mapping** based on error types

```csharp
// Success with value
return Result<string>.Success(task.Id);

// Failure with structured error
return Result.Failure(new Error(
    "UPDATE_004",
    "Completed tasks cannot be updated",
    "invalid_operation"));

// Validation errors with details
Error error = new Error("CREATE_TASK", "Missing mandatory fields", "validation");
error.AddDetails(validationErrors);
return Result<string>.Failure(error);
```

### 🔧 JSON Configuration

String-based enum serialization is configured globally:

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
```

## 🎓 What You'll Learn

By exploring this sample, you'll understand:

1. 🔧 **How to set up Dom.Mediator** in a Minimal API application
2. 🏛️ **CQRS implementation** with separate commands and queries
3. 🎯 **Unified pipeline behaviors** to avoid code duplication
4. 🏗️ **Rich domain models** with encapsulated business rules
5. 📐 **Domain-Driven Design** principles in practice
6. ✅ **Domain-level validation** vs handler-level validation
7. 📈 **Result pattern** for consistent error handling
8. 🌐 **HTTP integration** with proper status codes and responses
9. 🔄 **State machine patterns** for status transitions

## 🛠️ Technologies Used

- 🟣 **.NET 8.0**
- 🌐 **ASP.NET Core Minimal APIs**
- 🎯 **Dom.Mediator** - CQRS/Mediator implementation
- 📚 **Swagger/OpenAPI** - API documentation
- 📝 **System.Text.Json** - JSON serialization with enum support

## 🏗️ Architecture Principles

This sample demonstrates:

- **Domain-Driven Design (DDD)**
  - Rich domain models with business logic
  - Ubiquitous language (Status, TaskItem, etc.)

- **CQRS (Command Query Responsibility Segregation)**
  - Separate command and query models
  - Different interfaces for reads and writes

- **Single Responsibility Principle**
  - Handlers orchestrate, domain models enforce rules
  - Behaviors handle cross-cutting concerns

- **Dependency Inversion**
  - Abstractions over implementations
  - Mediator pattern for loose coupling

## 🚀 Next Steps

This sample provides a foundation for building more complex applications.

Consider extending it with:

- 🗃️ **Persistence**: Database integration (Entity Framework Core, Dapper)
- 🔐 **Security**: Authentication and authorization (JWT, OAuth)
- ✅ **Advanced Validation**: FluentValidation integration
- 📊 **Additional Behaviors**: Caching, performance monitoring, retry policies
- 🧪 **Testing**: Unit tests for domain logic, integration tests for handlers
- 🎯 **Domain Events**: Publish/subscribe pattern for side effects
- 📈 **Observability**: OpenTelemetry, structured logging
- 🔄 **State Persistence**: Event sourcing for audit trail
- ➕ **More Features**: Task assignment, priorities, tags, search

## 📖 Related Resources

- [Dom.Mediator Documentation](https://github.com/asantacroce/dom.mediator)
- [Domain-Driven Design Fundamentals](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Result Pattern in C#](https://enterprisecraftsmanship.com/posts/functional-c-handling-failures-input-errors/)
