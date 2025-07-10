using Dom.Mediator.Interfaces;
using Dom.Mediator.Interfaces.Handlers;
using Dom.Mediator.Models;
using Dom.Mediator.ResultPattern;
using FluentValidation;
using SimpleApi.Behaviours.FluentValidation;
using Moq;
using System.Reflection;

namespace Dom.Mediator.Test
{
    public class ValidationBehaviorTests
    {
        #region FluentValidationBehaviour Tests

        [Fact]
        public async Task FluentValidationBehaviour_WithNoValidators_CallsNext()
        {
            // Arrange
            var validators = new List<IValidator<TestRequest>>();
            var behavior = new FluentValidationBehaviour<TestRequest, string>(validators);
            
            bool nextWasCalled = false;
            RequestHandlerDelegate<string> next = () => 
            {
                nextWasCalled = true;
                return Task.FromResult(Result<string>.Success("Success"));
            };
            
            // Act
            var result = await behavior.Handle(new TestRequest(), CancellationToken.None, next);
            
            // Assert
            Assert.True(nextWasCalled);
            Assert.True(result.IsSuccess);
            Assert.Equal("Success", result.Value);
        }
        
        [Fact]
        public async Task FluentValidationBehaviour_WithPassingValidation_CallsNext()
        {
            // Arrange
            var mockValidator = new Mock<IValidator<TestRequest>>();
            mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
                
            var validators = new List<IValidator<TestRequest>> { mockValidator.Object };
            var behavior = new FluentValidationBehaviour<TestRequest, string>(validators);
            
            var expectedResult = Result<string>.Success("Success");
            RequestHandlerDelegate<string> next = () => Task.FromResult(expectedResult);
            
            // Act
            var result = await behavior.Handle(new TestRequest(), CancellationToken.None, next);
            
            // Assert
            Assert.Same(expectedResult, result);
            mockValidator.Verify(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task FluentValidationBehaviour_WithFailingValidation_ReturnsFailureResult()
        {
            // Arrange
            var mockValidator = new Mock<IValidator<TestRequest>>();
            var validationFailures = new List<FluentValidation.Results.ValidationFailure>
            {
                new FluentValidation.Results.ValidationFailure("Property", "Error message") 
                { 
                    ErrorCode = "ErrorCode" 
                }
            };
            
            mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));
                
            var validators = new List<IValidator<TestRequest>> { mockValidator.Object };
            var behavior = new FluentValidationBehaviour<TestRequest, string>(validators);
            
            bool nextWasCalled = false;
            RequestHandlerDelegate<string> next = () => 
            {
                nextWasCalled = true;
                return Task.FromResult(Result<string>.Success("Success"));
            };
            
            // Act
            var result = await behavior.Handle(new TestRequest(), CancellationToken.None, next);
            
            // Assert
            Assert.False(nextWasCalled);
            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.Errors[0].Type);
            Assert.Equal("ErrorCode", result.Errors[0].Code);
            Assert.Equal("Error message", result.Errors[0].Description);
        }
        
        [Fact]
        public async Task FluentValidationBehaviour_WithMultipleValidators_ValidatesAll()
        {
            // Arrange
            var mockValidator1 = new Mock<IValidator<TestRequest>>();
            mockValidator1.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult(new List<FluentValidation.Results.ValidationFailure> 
                {
                    new FluentValidation.Results.ValidationFailure("Property1", "Error 1") { ErrorCode = "Error1" }
                }));
                
            var mockValidator2 = new Mock<IValidator<TestRequest>>();
            mockValidator2.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult(new List<FluentValidation.Results.ValidationFailure> 
                {
                    new FluentValidation.Results.ValidationFailure("Property2", "Error 2") { ErrorCode = "Error2" }
                }));
                
            var validators = new List<IValidator<TestRequest>> { mockValidator1.Object, mockValidator2.Object };
            var behavior = new FluentValidationBehaviour<TestRequest, string>(validators);
            
            bool nextWasCalled = false;
            RequestHandlerDelegate<string> next = () => 
            {
                nextWasCalled = true;
                return Task.FromResult(Result<string>.Success("Success"));
            };
            
            // Act
            var result = await behavior.Handle(new TestRequest(), CancellationToken.None, next);
            
            // Assert
            Assert.False(nextWasCalled);
            Assert.True(result.IsFailure);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal("Error1", result.Errors[0].Code);
            Assert.Equal("Error 1", result.Errors[0].Description);
            Assert.Equal("Error2", result.Errors[1].Code);
            Assert.Equal("Error 2", result.Errors[1].Description);
            Assert.All(result.Errors, error => Assert.Equal(ErrorType.Validation, error.Type));
        }

        #endregion

        #region FluentValidationBehaviourForCommand Tests

        [Fact]
        public async Task FluentValidationBehaviourForCommand_WithNoValidators_CallsNext()
        {
            // Arrange
            var validators = new List<IValidator<TestCommand>>();
            var behavior = new FluentValidationBehaviourForCommand<TestCommand>(validators);
            
            bool nextWasCalled = false;
            CommandHandlerDelegate next = () => 
            {
                nextWasCalled = true;
                return Task.FromResult(Result.Success());
            };
            
            // Act
            var result = await behavior.Handle(new TestCommand(), CancellationToken.None, next);
            
            // Assert
            Assert.True(nextWasCalled);
            Assert.True(result.IsSuccess);
        }
        
        [Fact]
        public async Task FluentValidationBehaviourForCommand_WithPassingValidation_CallsNext()
        {
            // Arrange
            var mockValidator = new Mock<IValidator<TestCommand>>();
            mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
                
            var validators = new List<IValidator<TestCommand>> { mockValidator.Object };
            var behavior = new FluentValidationBehaviourForCommand<TestCommand>(validators);
            
            var expectedResult = Result.Success();
            CommandHandlerDelegate next = () => Task.FromResult(expectedResult);
            
            // Act
            var result = await behavior.Handle(new TestCommand(), CancellationToken.None, next);
            
            // Assert
            Assert.Same(expectedResult, result);
            mockValidator.Verify(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task FluentValidationBehaviourForCommand_WithFailingValidation_ReturnsFailureResult()
        {
            // Arrange
            var mockValidator = new Mock<IValidator<TestCommand>>();
            var validationFailures = new List<FluentValidation.Results.ValidationFailure>
            {
                new FluentValidation.Results.ValidationFailure("Property", "Error message") 
                { 
                    ErrorCode = "VALIDATION_ERROR" 
                }
            };
            
            mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));
                
            var validators = new List<IValidator<TestCommand>> { mockValidator.Object };
            var behavior = new FluentValidationBehaviourForCommand<TestCommand>(validators);
            
            bool nextWasCalled = false;
            CommandHandlerDelegate next = () => 
            {
                nextWasCalled = true;
                return Task.FromResult(Result.Success());
            };
            
            // Act
            var result = await behavior.Handle(new TestCommand(), CancellationToken.None, next);
            
            // Assert
            Assert.False(nextWasCalled);
            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.Errors[0].Type);
            Assert.Equal("VALIDATION_ERROR", result.Errors[0].Code);
            Assert.Contains("Error message", result.Errors[0].Description);
        }

        [Fact]
        public async Task FluentValidationBehaviourForCommand_CombinesMultipleErrorMessages()
        {
            // Arrange
            var mockValidator = new Mock<IValidator<TestCommand>>();
            var validationFailures = new List<FluentValidation.Results.ValidationFailure>
            {
                new FluentValidation.Results.ValidationFailure("Property1", "Error message 1"),
                new FluentValidation.Results.ValidationFailure("Property2", "Error message 2")
            };
            
            mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));
                
            var validators = new List<IValidator<TestCommand>> { mockValidator.Object };
            var behavior = new FluentValidationBehaviourForCommand<TestCommand>(validators);
            
            // Act
            var result = await behavior.Handle(new TestCommand(), CancellationToken.None, () => Task.FromResult(Result.Success()));
            
            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("VALIDATION_ERROR", result.Errors[0].Code);
            Assert.Contains("Error message 1", result.Errors[0].Description);
            Assert.Contains("Error message 2", result.Errors[0].Description);
            Assert.Contains(";", result.Errors[0].Description); // Check for the separator
        }

        #endregion

        #region Integration with Mediator Tests

        [Fact]
        public async Task Mediator_WithRegisteredFluentValidationBehaviour_ValidatesRequests()
        {
            // Arrange
            var mediator = new Mediator();
            mediator.ScanHandlers(Assembly.GetExecutingAssembly());
            
            // Create and register validator
            var mockValidator = new Mock<IValidator<TestRequest>>();
            mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[] 
                {
                    new FluentValidation.Results.ValidationFailure("Property", "Validation failed")
                    {
                        ErrorCode = "VALIDATION_ERROR",
                        ErrorMessage = "Validation failed"
                    }
                }));
                
            var validators = new List<IValidator<TestRequest>> { mockValidator.Object };
            var behavior = new FluentValidationBehaviour<TestRequest, string>(validators);
            
            mediator.AddRequestResponseBehaviour(behavior);
            
            // Act
            var result = await mediator.Send(new TestRequest());
            
            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.Errors[0].Type);
        }

        #endregion

        // Test request and command classes for validation behavior tests
        public class TestRequest : IQuery<string> { }
        
        public class TestRequestHandler : IQueryHandler<TestRequest, string>
        {
            public Task<Result<string>> Handle(TestRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult(Result<string>.Success("TestResponse"));
            }
        }
        
        public class TestCommand : ICommand { }
    }
}