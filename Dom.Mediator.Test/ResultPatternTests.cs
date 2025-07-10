using Dom.Mediator.ResultPattern;

namespace Dom.Mediator.Test
{
    public class ResultPatternTests
    {
        #region Result<T> Tests
        
        [Fact]
        public void ResultT_Success_SetsCorrectProperties()
        {
            // Arrange
            var value = "test value";
            
            // Act
            var result = Result<string>.Success(value);
            
            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal(value, result.Value);
            Assert.Empty(result.Errors);
        }
        
        [Fact]
        public void ResultT_FailureWithSingleError_SetsCorrectProperties()
        {
            // Arrange
            string errorCode = "ERROR_CODE";
            string errorDescription = "Error description";
            ErrorType errorType = ErrorType.Validation;
            
            // Act
            var result = Result<string>.Failure(errorCode, errorDescription, errorType);
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal(errorCode, result.Errors[0].Code);
            Assert.Equal(errorDescription, result.Errors[0].Description);
            Assert.Equal(errorType, result.Errors[0].Type);
        }
        
        [Fact]
        public void ResultT_FailureWithMultipleErrors_SetsCorrectProperties()
        {
            // Arrange
            var errors = new List<Error>
            {
                new Error("ERROR_1", "First error", ErrorType.Validation),
                new Error("ERROR_2", "Second error", ErrorType.NotFound)
            };
            
            // Act
            var result = Result<string>.Failure(errors);
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Null(result.Value);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal(errors, result.Errors);
        }
        
        #endregion
        
        #region Result Tests
        
        [Fact]
        public void Result_Success_SetsCorrectProperties()
        {
            // Act
            var result = Result.Success();
            
            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Empty(result.Errors);
        }
        
        [Fact]
        public void Result_FailureWithSingleError_SetsCorrectProperties()
        {
            // Arrange
            string errorCode = "ERROR_CODE";
            string errorDescription = "Error description";
            ErrorType errorType = ErrorType.Validation;
            
            // Act
            var result = Result.Failure(errorCode, errorDescription, errorType);
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Single(result.Errors);
            Assert.Equal(errorCode, result.Errors[0].Code);
            Assert.Equal(errorDescription, result.Errors[0].Description);
            Assert.Equal(errorType, result.Errors[0].Type);
        }
        
        [Fact]
        public void Result_FailureWithMultipleErrors_SetsCorrectProperties()
        {
            // Arrange
            var errors = new List<Error>
            {
                new Error("ERROR_1", "First error", ErrorType.Validation),
                new Error("ERROR_2", "Second error", ErrorType.NotFound)
            };
            
            // Act
            var result = Result.Failure(errors);
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal(errors, result.Errors);
        }
        
        #endregion
        
        #region Error Tests
        
        [Fact]
        public void Error_Constructor_SetsProperties()
        {
            // Arrange
            string code = "ERROR_CODE";
            string description = "Error description";
            ErrorType type = ErrorType.NotFound;
            
            // Act
            var error = new Error(code, description, type);
            
            // Assert
            Assert.Equal(code, error.Code);
            Assert.Equal(description, error.Description);
            Assert.Equal(type, error.Type);
        }
        
        [Fact]
        public void Error_WithDefaultErrorType_SetsTypeToUnknown()
        {
            // Act
            var error = new Error("CODE", "Description");
            
            // Assert
            Assert.Equal(ErrorType.Unknown, error.Type);
        }
        
        #endregion
    }
}