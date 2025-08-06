# 📊 Code Coverage Guide

This document explains how to view and interpret code coverage results for Dom.Mediator.

## 🔍 How to View Coverage Results

### 1. **GitHub Actions Summary** ⭐ (Recommended)
When the workflow runs, coverage results are automatically added to the GitHub Actions summary:
- Go to **Actions** tab in GitHub
- Click on the latest workflow run
- Scroll down to see the coverage summary with badges

### 2. **Pull Request Comments**
For pull requests, coverage results are automatically posted as comments:
- Coverage percentage breakdown
- Line-by-line coverage details
- Links to download full reports

### 3. **GitHub Pages** 🌐 (Interactive Browsing)
Full HTML coverage reports are published to GitHub Pages:
- **URL**: `https://[username].github.io/dom.mediator/coverage/`
- Navigate through files and see line-by-line coverage
- Interactive HTML with highlighting

### 4. **Downloadable Artifacts**
Download complete coverage reports from GitHub Actions:
- Go to **Actions** → Select workflow run
- Scroll to **Artifacts** section
- Download `coverage-report.zip`
- Extract and open `index.html` in browser

## 📈 Understanding Coverage Metrics

### **Line Coverage** (Primary Metric)
- **Green Lines**: Executed during tests
- **Red Lines**: Not executed during tests
- **Target**: 60%+ (configurable in workflow)

### **Branch Coverage**
- Measures decision points (if/else, switch, loops)
- Shows which code paths were tested

### **Method Coverage**  
- Percentage of methods that were called
- **Full Method Coverage**: Methods where every line was executed

## 🎯 Coverage Thresholds

Current thresholds (configurable in `.github/workflows/publish.yml`):
- **Minimum Line Coverage**: 60%
- **Warning Level**: Below threshold shows warning but doesn't fail build
- **Location**: Line 60 in publish workflow

## 📋 Current Coverage (Last Run)

| Metric | Percentage | Details |
|--------|------------|---------|
| **Line Coverage** | 66.3% | 81/122 lines covered |
| **Branch Coverage** | 61.5% | 32/52 branches covered |
| **Method Coverage** | 73.5% | 25/34 methods covered |

### Coverage by Class:
- `Dom.Mediator.Result<T>`: 92.3%
- `Dom.Mediator.Result`: 90.9%
- `Dom.Mediator.Error`: 81.8%
- `Dom.Mediator.Implementation.Mediator`: 67.6%
- `Dom.Mediator.Models.MediatorException`: 33.3%
- `Dom.Mediator.MediatorDependencyExtentions`: 0%
- `ErrorDetail`: 0%

## 🔧 Improving Coverage

### Areas Needing Attention:
1. **MediatorDependencyExtentions** (0% coverage)
   - Add integration tests for DI setup
   
2. **ErrorDetail** (0% coverage)
   - Add tests for error detail functionality
   
3. **MediatorException** (33.3% coverage)
   - Test exception scenarios and error handling

### Adding Tests:
```csharp
[Fact]
public void MediatorDependencyExtensions_ConfiguresCorrectly()
{
    var services = new ServiceCollection();
    services.AddMediator(config => {
        config.RegisterHandlers(Assembly.GetExecutingAssembly());
    });
    
    var serviceProvider = services.BuildServiceProvider();
    var mediator = serviceProvider.GetService<IMediator>();
    
    Assert.NotNull(mediator);
}
```

## 🚀 Local Coverage Testing

Run coverage locally:
```bash
# Navigate to project root
cd /path/to/dom.mediator

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./tests/TestResults

# Install report generator (one time)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator \
  -reports:"./tests/TestResults/**/coverage.cobertura.xml" \
  -targetdir:"./tests/CoverageReport" \
  -reporttypes:"Html;TextSummary"

# Open in browser
start ./tests/CoverageReport/index.html  # Windows
open ./tests/CoverageReport/index.html   # macOS
xdg-open ./tests/CoverageReport/index.html # Linux
```

## 📁 Coverage Report Files

The coverage report generates the following key files in `./tests/CoverageReport/`:

### **Interactive Reports**
- `index.html` - Main coverage dashboard
- `Dom.Mediator_*.html` - Per-class coverage details

### **Data Files**
- `Summary.txt` - Text summary for scripts
- `Summary.md` - Markdown summary for GitHub
- `Cobertura.xml` - Machine-readable coverage data

### **Visual Assets**
- `badge_*.svg` - Coverage badges for README
- `*.css`, `*.js` - Styling and interactive features

## 🔗 Integration Points

### **GitHub Workflows**
Coverage is integrated into CI/CD pipelines:
- **Test Job**: Collects coverage during test execution
- **Reporting**: Generates and uploads coverage artifacts
- **GitHub Pages**: Publishes interactive reports
- **PR Comments**: Posts coverage summaries automatically

### **Local Development**
Use coverage during development:
```bash
# Quick coverage check
dotnet test --collect:"XPlat Code Coverage"

# Detailed analysis
reportgenerator -reports:"./tests/TestResults/**/coverage.cobertura.xml" -targetdir:"./tests/CoverageReport" -reporttypes:"Html"
```

---

**🎯 Goal**: Maintain 80%+ line coverage while ensuring meaningful test scenarios that validate real-world usage patterns.