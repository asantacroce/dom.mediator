# 🧪 Dom.Mediator Tests

This directory contains all testing infrastructure, unit tests, and coverage reports for the Dom.Mediator library.

## 📁 Directory Structure

```
tests/
├── README.md                    # This file - testing documentation
├── COVERAGE.md                  # Detailed coverage guide and metrics
├── Dom.Mediator.Test/           # Unit test project
│   ├── Dom.Mediator.Test.csproj # Test project file
│   ├── MediatorTests.cs         # Core mediator functionality tests
│   └── ResultPatternTests.cs    # Result pattern tests
├── TestResults/                 # Generated test results (gitignored)
│   └── **/*.xml                # Test output and coverage data
└── CoverageReport/              # Generated coverage reports (gitignored)
    ├── index.html               # Interactive HTML coverage report
    ├── Summary.txt              # Text summary of coverage
    └── **/*                     # Detailed coverage files
```

## 🚀 Running Tests

### Local Development

```bash
# Run all tests
dotnet test

# Run with coverage collection
dotnet test --collect:"XPlat Code Coverage" --results-directory ./tests/TestResults

# Build and run tests
dotnet build && dotnet test --no-build
```

### Generate Coverage Reports

```bash
# Install ReportGenerator (one time)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML coverage report
reportgenerator \
  -reports:"./tests/TestResults/**/coverage.cobertura.xml" \
  -targetdir:"./tests/CoverageReport" \
  -reporttypes:"Html;TextSummary"

# Open coverage report
start ./tests/CoverageReport/index.html  # Windows
open ./tests/CoverageReport/index.html   # macOS
```

## 📊 Current Test Coverage

| Metric | Percentage | Details |
|--------|------------|---------|
| **Line Coverage** | 66.3% | 81/122 lines covered |
| **Branch Coverage** | 61.5% | 32/52 branches covered |
| **Method Coverage** | 73.5% | 25/34 methods covered |

> **📈 View detailed coverage**: See [COVERAGE.md](./COVERAGE.md) for comprehensive coverage analysis and improvement strategies.

## 🧪 Test Categories

### **Core Mediator Tests** (`MediatorTests.cs`)
- ✅ Query handling and response validation
- ✅ Command processing (with and without responses)
- ✅ Error handling and exception scenarios
- ✅ Handler registration and discovery
- ✅ Pipeline behavior integration

### **Result Pattern Tests** (`ResultPatternTests.cs`)
- ✅ Success result creation and validation
- ✅ Failure result handling with error details
- ✅ Result type conversions and matching
- ✅ Error object structure and properties

## 🔄 CI/CD Integration

Tests are automatically executed in GitHub Actions:

### **Publish Pipeline** (`.github/workflows/publish.yml`)
- 🔨 Builds solution in Release mode
- 🧪 Runs all tests with coverage collection
- 📊 Generates interactive HTML coverage reports
- 📈 Posts coverage summaries to GitHub Actions
- 💬 Comments coverage results on pull requests
- 🌐 Publishes coverage to GitHub Pages

### **Coverage Outputs**
1. **GitHub Actions Summary**: Coverage metrics displayed in workflow runs
2. **Pull Request Comments**: Automated coverage reports on PRs
3. **GitHub Pages**: Interactive coverage reports at `https://[username].github.io/dom.mediator/coverage/`
4. **Downloadable Artifacts**: Complete coverage reports as ZIP files

## 🎯 Test Quality Standards

### **Coverage Thresholds**
- **Minimum Line Coverage**: 60% (configurable)
- **Target Coverage**: 80%+
- **Branch Coverage**: Monitored and reported

### **Test Requirements**
- ✅ All public API methods must have tests
- ✅ Error scenarios must be covered
- ✅ Pipeline behaviors must be validated
- ✅ Integration patterns must be tested

## 📝 Adding New Tests

### Test Structure
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var mediator = CreateMediator();
    var request = new TestRequest { /* properties */ };
    
    // Act
    var result = await mediator.Send(request);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(expectedValue, result.Value);
}
```

### Helper Methods
- `CreateMediator()`: Sets up a configured mediator instance
- Use `Assert.NotNull(result.Error)` before accessing error properties
- Follow AAA pattern: Arrange, Act, Assert

## 🔗 Related Documentation

- **[Coverage Guide](./COVERAGE.md)**: Detailed coverage analysis and viewing instructions
- **[Main README](../README.md)**: Library overview and usage examples
- **[Samples](../samples/)**: Complete working examples

---

**📊 For detailed coverage metrics and viewing instructions, see [COVERAGE.md](./COVERAGE.md)**