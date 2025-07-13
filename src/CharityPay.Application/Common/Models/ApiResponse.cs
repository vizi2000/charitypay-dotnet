namespace CharityPay.Application.Common.Models;

/// <summary>
/// Standardized API response envelope.
/// </summary>
/// <typeparam name="T">The type of data being returned.</typeparam>
public record ApiResponse<T>
{
    /// <summary>
    /// Indicates if the request was successful.
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// The response data.
    /// </summary>
    public T? Data { get; init; }
    
    /// <summary>
    /// A message providing additional context about the response.
    /// </summary>
    public string? Message { get; init; }
    
    /// <summary>
    /// Validation errors if any.
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; init; }
    
    /// <summary>
    /// The timestamp of the response.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    /// <summary>
    /// Creates a failure response with error message.
    /// </summary>
    public static ApiResponse<T> FailureResponse(string message, Dictionary<string, string[]>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}

/// <summary>
/// Paginated response model.
/// </summary>
public record PaginatedResponse<T>
{
    /// <summary>
    /// The items in the current page.
    /// </summary>
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    
    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int CurrentPage { get; init; }
    
    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; init; }
    
    /// <summary>
    /// The total number of items.
    /// </summary>
    public int TotalCount { get; init; }
    
    /// <summary>
    /// The total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    
    /// <summary>
    /// Indicates if there is a previous page.
    /// </summary>
    public bool HasPrevious => CurrentPage > 1;
    
    /// <summary>
    /// Indicates if there is a next page.
    /// </summary>
    public bool HasNext => CurrentPage < TotalPages;
}