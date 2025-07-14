# CharityPay .NET Testing Infrastructure

## Overview

The CharityPay .NET project includes a comprehensive testing infrastructure designed to ensure code quality, reliability, and performance. The testing framework follows Clean Architecture principles and includes multiple testing layers.

## Testing Architecture

### Test Project Structure

```
tests/
├── CharityPay.Domain.Tests/           # Domain entity and business logic tests
├── CharityPay.Application.Tests/      # Service and use case tests
├── CharityPay.Infrastructure.Tests/   # Database and external service tests
├── CharityPay.API.Tests/             # Integration and API endpoint tests
├── TestRunner.ps1                    # PowerShell test runner
├── TestRunner.sh                     # Bash test runner
└── TestResults/                      # Generated test results and coverage
```

### Testing Frameworks and Tools

- **xUnit 2.6.4** - Primary testing framework
- **FluentAssertions 6.12.0** - Readable and comprehensive assertions
- **Moq 4.20.70** - Mocking framework for dependencies
- **Bogus 35.2.0** - Test data generation
- **TestContainers.PostgreSql 3.6.0** - Real database testing
- **Microsoft.AspNetCore.Mvc.Testing 8.0.1** - API integration testing
- **Coverlet** - Code coverage collection

## Test Categories

### 1. Unit Tests
- **Purpose**: Test individual components in isolation
- **Scope**: Domain entities, business logic, services
- **Category**: `[Category("Unit")]`
- **Characteristics**: Fast, isolated, no external dependencies

### 2. Integration Tests
- **Purpose**: Test component interactions
- **Scope**: Database operations, API endpoints, service integrations
- **Category**: `[Category("Integration")]`
- **Characteristics**: Use real databases (TestContainers), HTTP clients

### 3. Performance Tests
- **Purpose**: Validate system performance under load
- **Scope**: API endpoints, database operations
- **Category**: `[Speed("Slow")]`
- **Characteristics**: Load testing, response time validation, throughput measurement

## Key Testing Features

### 1. Test Data Builders
Located in `CharityPay.Domain.Tests/Builders/`, these provide fluent APIs for creating test data:

```csharp
var user = TestDataBuilders.User()
    .AsAdmin()
    .WithEmail("admin@test.com")
    .Build();

var organization = TestDataBuilders.Organization()
    .WithUser(user)
    .AsApproved()
    .Build();
```

### 2. Test Fixtures
- **ApplicationTestFixture**: In-memory database with AutoMapper
- **DatabaseTestFixture**: Real PostgreSQL with TestContainers
- **ApiTestFixture**: Full API integration with authentication

### 3. Performance Testing Framework
```csharp
var result = await ExecuteLoadTestAsync(
    client => client.GetAsync("/api/v1/organizations"),
    concurrentUsers: 20,
    requestsPerUser: 10);

AssertPerformanceCriteria(
    result,
    minSuccessRate: 0.95,
    maxAverageResponseTime: TimeSpan.FromMilliseconds(500));
```

### 4. Database Integration Testing
```csharp
[Collection(nameof(DatabaseTestCollection))]
public class RepositoryTests : DatabaseTestBase
{
    // Tests with real PostgreSQL database
    // Automatic cleanup between tests
    // Transaction rollback support
}
```

### 5. API Integration Testing
```csharp
[Collection(nameof(ApiTestCollection))]
public class ApiTests : ApiTestBase
{
    // Full HTTP client testing
    // Authentication helpers
    // Real database backend
}
```

## Running Tests

### Command Line

#### Using Test Runners
```bash
# Run all tests
./tests/TestRunner.sh

# Run specific test types
./tests/TestRunner.sh -t Unit
./tests/TestRunner.sh -t Integration
./tests/TestRunner.sh -t Performance

# Run with coverage
./tests/TestRunner.sh -c --open

# Run specific projects
./tests/TestRunner.sh -p Domain -v
```

#### Using .NET CLI
```bash
# All tests
dotnet test

# Specific project
dotnet test tests/CharityPay.Domain.Tests/

# With filter
dotnet test --filter "Category=Unit"

# With coverage
dotnet test --collect "XPlat Code Coverage"
```

### IDE Integration
- Visual Studio: Test Explorer
- VS Code: .NET Core Test Explorer extension
- JetBrains Rider: Built-in test runner

## Test Runner Features

### Cross-Platform Support
- **TestRunner.ps1**: PowerShell (Windows, PowerShell Core)
- **TestRunner.sh**: Bash (macOS, Linux)

### Features
- ✅ Test categorization and filtering
- ✅ Code coverage with ReportGenerator
- ✅ Parallel test execution
- ✅ Performance measurement
- ✅ Results aggregation
- ✅ Automatic report generation

### Usage Examples
```bash
# Quick unit tests
./TestRunner.sh -t Unit -p Domain

# Full integration test suite
./TestRunner.sh -t Integration -c -v

# Performance testing
./TestRunner.sh -t Performance --no-parallel

# Coverage report
./TestRunner.sh -c --open
```

## Testing Best Practices

### 1. Test Organization
- One test class per entity/service
- Descriptive test method names
- Arrange-Act-Assert pattern
- Use test categories for filtering

### 2. Test Data Management
- Use test builders for consistent data
- Clean database state between tests
- Avoid hard-coded test data
- Use realistic but simple test scenarios

### 3. Assertions
- Use FluentAssertions for readability
- Test behavior, not implementation
- Assert on meaningful outcomes
- Include descriptive error messages

### 4. Performance Testing
- Test realistic scenarios
- Measure what matters
- Set appropriate thresholds
- Monitor trends over time

## Code Coverage

### Coverage Targets
- **Overall**: >80%
- **Domain Layer**: >90%
- **Application Layer**: >80%
- **Infrastructure Layer**: >70%
- **API Layer**: >75%

### Coverage Reports
Generated automatically with:
- HTML reports in `TestResults/coverage-report/`
- Summary statistics
- Line and branch coverage
- Coverage badges

## Continuous Integration

### Test Execution Strategy
1. **Pull Request**: Unit tests only
2. **Main Branch**: Unit + Integration tests
3. **Release**: All tests including performance
4. **Nightly**: Full test suite with coverage

### Quality Gates
- All tests must pass
- Coverage thresholds must be met
- Performance tests must not regress
- No critical security vulnerabilities

## Troubleshooting

### Common Issues

#### Database Connection Issues
```bash
# Check Docker is running
docker ps

# Reset test database
./TestRunner.sh -t Integration --recreate-db
```

#### Test Discovery Issues
```bash
# Clean and rebuild
dotnet clean
dotnet build

# Check test project references
dotnet list package
```

#### Performance Test Failures
```bash
# Run with verbose logging
./TestRunner.sh -t Performance -v

# Check system resources
top
free -h
```

### Test Debugging
- Use `[Fact(Skip = "Debugging")]` to temporarily skip tests
- Add console output with `Console.WriteLine()` for debugging
- Use debugger breakpoints in test methods
- Check test output for detailed error messages

## Contributing to Tests

### Adding New Tests
1. Follow naming conventions: `MethodName_Scenario_ExpectedResult`
2. Use appropriate test categories
3. Include both positive and negative test cases
4. Add performance tests for critical paths

### Test Data Builders
1. Extend existing builders in `TestDataBuilders` class
2. Use fluent API pattern
3. Provide sensible defaults
4. Support method chaining

### Performance Tests
1. Inherit from `LoadTestBase`
2. Define realistic load patterns
3. Set appropriate performance criteria
4. Document test scenarios

## Metrics and Reporting

### Test Metrics
- Test execution time
- Coverage percentages
- Performance benchmarks
- Failure rates

### Reports Generated
- Test result summaries (TRX format)
- Coverage reports (HTML + XML)
- Performance test results
- Trend analysis over time

## Future Enhancements

### Planned Improvements
- [ ] Automated visual regression testing
- [ ] Contract testing with Pact
- [ ] Chaos engineering tests
- [ ] Security penetration tests
- [ ] End-to-end browser automation
- [ ] Database migration testing
- [ ] Multi-environment test orchestration

### Tools Under Consideration
- **SpecFlow**: Behavior-driven development
- **NBomber**: Advanced load testing
- **WireMock**: HTTP mocking
- **Playwright**: Browser automation
- **OWASP ZAP**: Security testing