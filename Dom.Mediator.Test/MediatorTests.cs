using Dom.Mediator.Interfaces;
using Dom.Mediator.Interfaces.Handlers;
using Dom.Mediator.Models;
using Dom.Mediator.ResultPattern;
using FluentValidation;
using SimpleApi.Behaviours.FluentValidation;
using System.Reflection;
using Moq;

namespace Dom.Mediator.Test
{
    public class MediatorTests
    {
        #region Query Tests
        
        [Fact]
        public async Task Query_WithValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            var mediator = new Mediator();
            mediator.ScanHandlers(Assembly.GetExecutingAssembly());
            
            var query = new TestQuery { Id = 1 };
            
            // Act
            var result = await mediator.Send(query);
            
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal($"TestResult-{query.Id}", result.Value);
        }
        
        [Fact]
        public async Task Query_WithNoHandler_ThrowsException()
        {
            // Arrange
            var mediator = new Mediator();
            var query = new NoHandlerQuery { Id = 1 };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await mediator.Send(query));
            
            Assert.Contains("Handler not registered for type", exception.Message);
            Assert.Contains("NoHandlerQuery", exception.Message);
        }
        
        #endregion

        #region Command With Response Tests
        
        [Fact]
        public async Task CommandWithResponse_WithValidCommand_ReturnsSuccessResult()
        {
            // Arrange
            var mediator = new Mediator();
            mediator.ScanHandlers(Assembly.GetExecutingAssembly());
            
            var command = new TestCommandWithResponse { Value = "test" };
            
            // Act
            var result = await mediator.Send(command);
            
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Response: test", result.Value);
        }
        
        [Fact]
        public async Task CommandWithResponse_WithHandlerReturningError_ReturnsFailureResult()
        {
            // Arrange
            var mediator = new Mediator();
            mediator.ScanHandlers(Assembly.GetExecutingAssembly());
            
            var command = new TestCommandWithResponse { Value = "error" };
            
            // Act
            var result = await mediator.Send(command);
            
            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
            Assert.Equal("Error occurred", result.Errors[0].Description);
            Assert.Equal(ErrorType.Validation, result.Errors[0].Type);
        }
        
        #endregion

        #region Command Without Response Tests
        
        [Fact]
        public async Task CommandWithoutResponse_WithValidCommand_ReturnsSuccessResult()
        {
            // Arrange
            var mediator = new Mediator();
            mediator.ScanHandlers(Assembly.GetExecutingAssembly());
            
            var command = new TestCommand { Value = "test" };
            
            // Act
            var result = await mediator.Send(command);
            
            // Assert
            Assert.True(result.IsSuccess);
        }
        
        [Fact]
        public async Task CommandWithoutResponse_WithHandlerReturningError_ReturnsFailureResult()
        {
            // Arrange
            var mediator = new Mediator();
            mediator.ScanHandlers(Assembly.GetExecutingAssembly());
            
            var command = new TestCommand { Value = "error" };
            
            // Act
            var result = await mediator.Send(command);
            
            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
            Assert.Equal("Error occurred", result.Errors[0].Description);
        }
        
        #endregion

        #region Validation Behavior Tests
        
        [Fact]
        public async Task ValidationBehavior_WithValidRequest_PassesThroughToHandler()
        {
            // Arrange
            var mediator = new Mediator();
            mediator.ScanHandlers(Assembly.GetExecutingAssembly());
            
            var mockValidator = new Mock<IValidator<TestQuery>>();
            mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestQuery>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
            var validators = new List<IValidator<TestQuery>> { mockValidator.Object };
            var behavior = new FluentValidationBehaviour<TestQuery, string>(validators);
            
            mediator.AddRequestResponseBehaviour(behavior);
            
            var query = new TestQuery { Id = 1 };
            
            // Act
            var result = await mediator.Send(query);
            
            // Assert
            Assert.True(result.IsSuccess);
            mockValidator.Verify(v => v.ValidateAsync(It.IsAny<ValidationContext<TestQuery>>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ValidationBehavior_WithInvalidRequest_ReturnsFailureResult()
        {
            // Arrange
            var mediator = new Mediator();
            mediator.ScanHandlers(Assembly.GetExecutingAssembly());
            
            var mockValidator = new Mock<IValidator<TestQuery>>();
            var validationFailures = new List<FluentValidation.Results.ValidationFailure>
            {
                new FluentValidation.Results.ValidationFailure("Id", "Id must be greater than 0") 
                { 
                    ErrorCode = "GreaterThanZero" 
                }
            };
            
            mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestQuery>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));
            
            var validators = new List<IValidator<TestQuery>> { mockValidator.Object };
            var behavior = new FluentValidationBehaviour<TestQuery, string>(validators);
            
            mediator.AddRequestResponseBehaviour(behavior);
            
            var query = new TestQuery { Id = 0 };
            
            // Act
            var result = await mediator.Send(query);
            
            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.Errors[0].Type);
            Assert.Equal("GreaterThanZero", result.Errors[0].Code);
            Assert.Equal("Id must be greater than 0", result.Errors[0].Description);
        }
        
        [Fact]
        public async Task ValidationBehaviorForCommand_WithInvalidCommand_ReturnsFailureResult()
        {
            // Arrange
            var mediator = new Mediator();
            mediator.ScanHandlers(Assembly.GetExecutingAssembly());
            
            var mockValidator = new Mock<IValidator<TestCommand>>();
            var validationFailures = new List<FluentValidation.Results.ValidationFailure>
            {
                new FluentValidation.Results.ValidationFailure("Value", "Value cannot be empty") 
                { 
                    ErrorCode = "NotEmpty" 
                }
            };
            
            mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));
            
            var validators = new List<IValidator<TestCommand>> { mockValidator.Object };
            var behavior = new FluentValidationBehaviourForCommand<TestCommand>(validators);
            
            mediator.AddRequestBehaviour(behavior);
            
            var command = new TestCommand { Value = string.Empty };
            
            // Act
            var result = await mediator.Send(command);
            
            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.Errors[0].Type);
            Assert.Contains("Value cannot be empty", result.Errors[0].Description);
        }
        
        [Fact]
        public async Task ValidationBehaviorForCommand_WithMultipleValidationErrors_ReturnsAllErrors()
        {
            // Arrange
            var mediator = new Mediator();
            mediator.ScanHandlers(Assembly.GetExecutingAssembly());
            
            var mockValidator = new Mock<IValidator<TestCommand>>();
            var validationFailures = new List<FluentValidation.Results.ValidationFailure>
            {
                new FluentValidation.Results.ValidationFailure("Value", "Value cannot be empty") { ErrorCode = "NotEmpty" },
                new FluentValidation.Results.ValidationFailure("Value", "Value must be at least 3 characters") { ErrorCode = "MinimumLength" }
            };
            
            mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));
            
            var validators = new List<IValidator<TestCommand>> { mockValidator.Object };
            var behavior = new FluentValidationBehaviourForCommand<TestCommand>(validators);
            
            mediator.AddRequestBehaviour(behavior);
            
            var command = new TestCommand { Value = string.Empty };
            
            // Act
            var result = await mediator.Send(command);
            
            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.Errors[0].Type);
            Assert.Contains("Value cannot be empty", result.Errors[0].Description);
            Assert.Contains("Value must be at least 3 characters", result.Errors[0].Description);
        }
        
        #endregion

        #region Multiple Behaviors Tests
        
        [Fact]
        public async Task MultipleBehaviors_ExecuteInCorrectOrder()
        {
            // Arrange
            var mediator = new Mediator();
            mediator.ScanHandlers(Assembly.GetExecutingAssembly());
            
            var executionOrder = new List<string>();
            
            // Create behaviors that record their execution order
            var behavior1 = new TestBehavior<TestQuery, string>("Behavior1", executionOrder);
            var behavior2 = new TestBehavior<TestQuery, string>("Behavior2", executionOrder);
            
            // Register behaviors in order (last registered = first executed)
            mediator.AddRequestResponseBehaviour(behavior1);
            mediator.AddRequestResponseBehaviour(behavior2);
            
            var query = new TestQuery { Id = 1 };
            
            // Act
            var result = await mediator.Send(query);
            
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(4, executionOrder.Count);
            Assert.Equal("Behavior1 Before", executionOrder[0]);
            Assert.Equal("Behavior2 Before", executionOrder[1]);
            Assert.Equal("Behavior2 After", executionOrder[2]);
            Assert.Equal("Behavior1 After", executionOrder[3]);
        }
        
        #endregion
    }

    #region Test Classes
    
    // Test Query
    public class TestQuery : IQuery<string>
    {
        public int Id { get; set; }
    }
    
    public class TestQueryHandler : IQueryHandler<TestQuery, string>
    {
        public Task<Result<string>> Handle(TestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success($"TestResult-{request.Id}"));
        }
    }
    
    public class NoHandlerQuery : IQuery<string>
    {
        public int Id { get; set; }
    }
    
    // Test Command with Response
    public class TestCommandWithResponse : ICommand<string>
    {
        public string Value { get; set; } = string.Empty;
    }
    
    public class TestCommandWithResponseHandler : ICommandHandler<TestCommandWithResponse, string>
    {
        public Task<Result<string>> Handle(TestCommandWithResponse request, CancellationToken cancellationToken)
        {
            if (request.Value == "error")
            {
                return Task.FromResult(Result<string>.Failure("ERROR_CODE", "Error occurred", ErrorType.Validation));
            }
            
            return Task.FromResult(Result<string>.Success($"Response: {request.Value}"));
        }
    }
    
    // Test Command without Response
    public class TestCommand : ICommand
    {
        public string Value { get; set; } = string.Empty;
    }
    
    public class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public Task<Result> Handle(TestCommand request, CancellationToken cancellationToken)
        {
            if (request.Value == "error")
            {
                return Task.FromResult(Result.Failure("ERROR_CODE", "Error occurred"));
            }
            
            return Task.FromResult(Result.Success());
        }
    }
    
    // Test Behavior for tracking execution order
    public class TestBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly string _name;
        private readonly List<string> _executionOrder;
        
        public TestBehavior(string name, List<string> executionOrder)
        {
            _name = name;
            _executionOrder = executionOrder;
        }
        
        public async Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            _executionOrder.Add($"{_name} Before");
            var result = await next();
            _executionOrder.Add($"{_name} After");
            return result;
        }
    }
    
    #endregion
}
