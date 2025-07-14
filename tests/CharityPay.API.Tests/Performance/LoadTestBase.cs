using CharityPay.API.Tests.Integration;
using System.Collections.Concurrent;
using System.Diagnostics;
using Xunit;
using FluentAssertions;

namespace CharityPay.API.Tests.Performance;

/// <summary>
/// Base class for load testing with performance measurement capabilities
/// </summary>
public abstract class LoadTestBase : ApiTestBase
{
    protected LoadTestBase(ApiTestFixture fixture) : base(fixture)
    {
    }

    /// <summary>
    /// Represents the result of a load test
    /// </summary>
    public class LoadTestResult
    {
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan MinResponseTime { get; set; }
        public TimeSpan MaxResponseTime { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public TimeSpan MedianResponseTime { get; set; }
        public double RequestsPerSecond { get; set; }
        public List<TimeSpan> ResponseTimes { get; set; } = new();
        public Dictionary<int, int> StatusCodeCounts { get; set; } = new();

        public TimeSpan GetPercentile(double percentile)
        {
            if (!ResponseTimes.Any()) return TimeSpan.Zero;
            
            var sorted = ResponseTimes.OrderBy(t => t).ToList();
            var index = (int)Math.Ceiling(percentile / 100.0 * sorted.Count) - 1;
            index = Math.Max(0, Math.Min(index, sorted.Count - 1));
            return sorted[index];
        }
    }

    /// <summary>
    /// Executes a load test with the specified parameters
    /// </summary>
    protected async Task<LoadTestResult> ExecuteLoadTestAsync(
        Func<HttpClient, Task<HttpResponseMessage>> requestAction,
        int concurrentUsers = 10,
        int requestsPerUser = 10,
        TimeSpan? testDuration = null)
    {
        var results = new ConcurrentBag<(bool Success, TimeSpan ResponseTime, int StatusCode)>();
        var overallStopwatch = Stopwatch.StartNew();
        var cancellationToken = testDuration.HasValue 
            ? new CancellationTokenSource(testDuration.Value).Token 
            : CancellationToken.None;

        var tasks = new List<Task>();

        // Create concurrent user tasks
        for (int user = 0; user < concurrentUsers; user++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (int request = 0; request < requestsPerUser && !cancellationToken.IsCancellationRequested; request++)
                {
                    var stopwatch = Stopwatch.StartNew();
                    try
                    {
                        var response = await requestAction(Client);
                        stopwatch.Stop();
                        
                        results.Add((
                            Success: response.IsSuccessStatusCode,
                            ResponseTime: stopwatch.Elapsed,
                            StatusCode: (int)response.StatusCode
                        ));
                    }
                    catch
                    {
                        stopwatch.Stop();
                        results.Add((
                            Success: false,
                            ResponseTime: stopwatch.Elapsed,
                            StatusCode: 500
                        ));
                    }
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);
        overallStopwatch.Stop();

        // Analyze results
        var resultsList = results.ToList();
        var responseTimes = resultsList.Select(r => r.ResponseTime).ToList();
        var successfulRequests = resultsList.Count(r => r.Success);
        var statusCodeCounts = resultsList
            .GroupBy(r => r.StatusCode)
            .ToDictionary(g => g.Key, g => g.Count());

        return new LoadTestResult
        {
            TotalRequests = resultsList.Count,
            SuccessfulRequests = successfulRequests,
            FailedRequests = resultsList.Count - successfulRequests,
            TotalDuration = overallStopwatch.Elapsed,
            MinResponseTime = responseTimes.Any() ? responseTimes.Min() : TimeSpan.Zero,
            MaxResponseTime = responseTimes.Any() ? responseTimes.Max() : TimeSpan.Zero,
            AverageResponseTime = responseTimes.Any() 
                ? TimeSpan.FromTicks((long)responseTimes.Average(t => t.Ticks)) 
                : TimeSpan.Zero,
            MedianResponseTime = responseTimes.Any() 
                ? responseTimes.OrderBy(t => t).Skip(responseTimes.Count / 2).First() 
                : TimeSpan.Zero,
            RequestsPerSecond = resultsList.Count / overallStopwatch.Elapsed.TotalSeconds,
            ResponseTimes = responseTimes,
            StatusCodeCounts = statusCodeCounts
        };
    }

    /// <summary>
    /// Asserts that load test results meet performance criteria
    /// </summary>
    protected static void AssertPerformanceCriteria(
        LoadTestResult result,
        double minSuccessRate = 0.95,
        TimeSpan? maxAverageResponseTime = null,
        TimeSpan? max95thPercentile = null,
        double? minRequestsPerSecond = null)
    {
        var successRate = (double)result.SuccessfulRequests / result.TotalRequests;
        successRate.Should().BeGreaterOrEqualTo(minSuccessRate, 
            $"Success rate should be at least {minSuccessRate:P}");

        if (maxAverageResponseTime.HasValue)
        {
            result.AverageResponseTime.Should().BeLessOrEqualTo(maxAverageResponseTime.Value,
                $"Average response time should be less than {maxAverageResponseTime.Value}");
        }

        if (max95thPercentile.HasValue)
        {
            var percentile95 = result.GetPercentile(95);
            percentile95.Should().BeLessOrEqualTo(max95thPercentile.Value,
                $"95th percentile response time should be less than {max95thPercentile.Value}");
        }

        if (minRequestsPerSecond.HasValue)
        {
            result.RequestsPerSecond.Should().BeGreaterOrEqualTo(minRequestsPerSecond.Value,
                $"Requests per second should be at least {minRequestsPerSecond.Value}");
        }
    }

    /// <summary>
    /// Prints detailed load test results to console
    /// </summary>
    protected static void PrintLoadTestResults(LoadTestResult result, string testName = null)
    {
        Console.WriteLine($"\n=== Load Test Results {(testName != null ? $"({testName})" : "")} ===");
        Console.WriteLine($"Total Requests: {result.TotalRequests}");
        Console.WriteLine($"Successful Requests: {result.SuccessfulRequests}");
        Console.WriteLine($"Failed Requests: {result.FailedRequests}");
        Console.WriteLine($"Success Rate: {(double)result.SuccessfulRequests / result.TotalRequests:P2}");
        Console.WriteLine($"Total Duration: {result.TotalDuration}");
        Console.WriteLine($"Requests/Second: {result.RequestsPerSecond:F2}");
        Console.WriteLine($"\nResponse Times:");
        Console.WriteLine($"  Min: {result.MinResponseTime.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Max: {result.MaxResponseTime.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Average: {result.AverageResponseTime.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Median: {result.MedianResponseTime.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  95th Percentile: {result.GetPercentile(95).TotalMilliseconds:F2}ms");
        Console.WriteLine($"  99th Percentile: {result.GetPercentile(99).TotalMilliseconds:F2}ms");
        
        Console.WriteLine($"\nStatus Code Distribution:");
        foreach (var kvp in result.StatusCodeCounts.OrderBy(x => x.Key))
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value} ({(double)kvp.Value / result.TotalRequests:P2})");
        }
        Console.WriteLine("===========================================\n");
    }

    /// <summary>
    /// Creates a performance test scenario that gradually increases load
    /// </summary>
    protected async Task<Dictionary<int, LoadTestResult>> ExecuteScalingTestAsync(
        Func<HttpClient, Task<HttpResponseMessage>> requestAction,
        int[] userCounts = null,
        int requestsPerUser = 10)
    {
        userCounts ??= new[] { 1, 5, 10, 20, 50 };
        var results = new Dictionary<int, LoadTestResult>();

        foreach (var userCount in userCounts)
        {
            Console.WriteLine($"Testing with {userCount} concurrent users...");
            var result = await ExecuteLoadTestAsync(requestAction, userCount, requestsPerUser);
            results[userCount] = result;
            
            // Brief pause between test runs
            await Task.Delay(1000);
        }

        return results;
    }

    /// <summary>
    /// Tests sustained load over a period of time
    /// </summary>
    protected async Task<LoadTestResult> ExecuteSustainedLoadTestAsync(
        Func<HttpClient, Task<HttpResponseMessage>> requestAction,
        int concurrentUsers = 10,
        TimeSpan duration = default)
    {
        if (duration == default)
            duration = TimeSpan.FromMinutes(2);

        return await ExecuteLoadTestAsync(
            requestAction, 
            concurrentUsers, 
            int.MaxValue, // Unlimited requests per user
            duration);
    }
}

/// <summary>
/// Specific load test scenarios for the CharityPay API
/// </summary>
public class CharityPayLoadTests : LoadTestBase
{
    public CharityPayLoadTests(ApiTestFixture fixture) : base(fixture)
    {
    }

    [SlowTest]
    public async Task GetOrganizations_LoadTest_ShouldHandleConcurrentRequests()
    {
        // Act
        var result = await ExecuteLoadTestAsync(
            client => client.GetAsync("/api/v1/organizations"),
            concurrentUsers: 20,
            requestsPerUser: 10);

        // Assert
        PrintLoadTestResults(result, "Get Organizations");
        AssertPerformanceCriteria(
            result,
            minSuccessRate: 0.95,
            maxAverageResponseTime: TimeSpan.FromMilliseconds(500),
            max95thPercentile: TimeSpan.FromSeconds(1),
            minRequestsPerSecond: 10);
    }

    [SlowTest]
    public async Task GetOrganizationById_LoadTest_ShouldMaintainPerformance()
    {
        // Arrange
        using var context = Fixture.GetDbContext();
        var organizationId = context.Organizations.First().Id;

        // Act
        var result = await ExecuteLoadTestAsync(
            client => client.GetAsync($"/api/v1/organizations/{organizationId}"),
            concurrentUsers: 15,
            requestsPerUser: 20);

        // Assert
        PrintLoadTestResults(result, "Get Organization By ID");
        AssertPerformanceCriteria(
            result,
            minSuccessRate: 0.98,
            maxAverageResponseTime: TimeSpan.FromMilliseconds(300));
    }

    [SlowTest]
    public async Task ApiEndpoints_ScalingTest_ShouldScaleLinear()
    {
        // Act
        var results = await ExecuteScalingTestAsync(
            client => client.GetAsync("/api/v1/organizations"),
            userCounts: new[] { 1, 5, 10, 20 },
            requestsPerUser: 5);

        // Assert
        foreach (var kvp in results)
        {
            PrintLoadTestResults(kvp.Value, $"Scaling Test - {kvp.Key} users");
            
            // Each scaling level should maintain good performance
            AssertPerformanceCriteria(
                kvp.Value,
                minSuccessRate: 0.95,
                maxAverageResponseTime: TimeSpan.FromSeconds(1));
        }
    }

    [SlowTest]
    public async Task AuthenticatedEndpoints_LoadTest_ShouldHandleAuthLoad()
    {
        // Arrange
        await AuthenticateAsOrganizationAsync();

        // Act
        var result = await ExecuteLoadTestAsync(
            client => client.GetAsync("/api/v1/organization/profile"),
            concurrentUsers: 10,
            requestsPerUser: 15);

        // Assert
        PrintLoadTestResults(result, "Authenticated Endpoints");
        AssertPerformanceCriteria(
            result,
            minSuccessRate: 0.95,
            maxAverageResponseTime: TimeSpan.FromMilliseconds(400));
    }

    [SlowTest]
    public async Task Mixed_WorkloadTest_ShouldHandleRealisticUsage()
    {
        // Arrange
        using var context = Fixture.GetDbContext();
        var organizationId = context.Organizations.First().Id;
        
        var endpoints = new[]
        {
            "/api/v1/organizations",
            $"/api/v1/organizations/{organizationId}",
            $"/api/v1/organizations/{organizationId}/stats",
            "/api/v1/health"
        };

        var random = new Random();

        // Act
        var result = await ExecuteLoadTestAsync(
            client =>
            {
                var endpoint = endpoints[random.Next(endpoints.Length)];
                return client.GetAsync(endpoint);
            },
            concurrentUsers: 25,
            requestsPerUser: 8);

        // Assert
        PrintLoadTestResults(result, "Mixed Workload");
        AssertPerformanceCriteria(
            result,
            minSuccessRate: 0.90, // Slightly lower for mixed workload
            maxAverageResponseTime: TimeSpan.FromMilliseconds(600));
    }
}