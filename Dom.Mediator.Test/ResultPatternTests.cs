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
        }
        
        [Fact]
        public void ResultT_FailureWithSingleError_SetsCorrectProperties()
        {
            // Arrange
            string errorCode = "ERROR_CODE";
            string errorDescription = "Error description";
            string errorType = "Validation";
            
            // Act
            var result = Result<string>.Failure(errorCode, errorDescription, errorType);
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Null(result.Value);
            Assert.Equal(errorCode, result.Error.Code);
            Assert.Equal(errorDescription, result.Error.Description);
            Assert.Equal(errorType, result.Error.Type);
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
        }
        
        [Fact]
        public void Result_FailureWithSingleError_SetsCorrectProperties()
        {
            // Arrange
            string errorCode = "ERROR_CODE";
            string errorDescription = "Error description";
            string errorType = "Validation";
            
            // Act
            var result = Result.Failure(errorCode, errorDescription, errorType);
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(errorCode, result.Error.Code);
            Assert.Equal(errorDescription, result.Error.Description);
            Assert.Equal(errorType, result.Error.Type);
        }
        
        #endregion
        
        #region Error Tests
        
        [Fact]
        public void Error_Constructor_SetsProperties()
        {
            // Arrange
            string code = "ERROR_CODE";
            string description = "Error description";
            string type = "NotFound";
            
            // Act
            var error = new Error(code, description, type);
            
            // Assert
            Assert.Equal(code, error.Code);
            Assert.Equal(description, error.Description);
            Assert.Equal(type, error.Type);
        }
        
        #endregion
    }
}