using Dom.Mediator.Interfaces;
using Dom.Mediator.Interfaces.Handlers;
using Dom.Mediator.Models;
using Dom.Mediator.ResultPattern;
using System.Reflection;

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
