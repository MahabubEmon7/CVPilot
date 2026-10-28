namespace CVPilot.Application.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string error) =>
        new() { Success = false, Errors = new List<string> { error } };

    public static ApiResponse<T> Fail(IEnumerable<string> errors) =>
        new() { Success = false, Errors = errors.ToList() };
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse Ok(string? message = null) =>
        new() { Success = true, Message = message };

    public static ApiResponse Fail(string error) =>
        new() { Success = false, Errors = new List<string> { error } };

    public static ApiResponse Fail(IEnumerable<string> errors) =>
        new() { Success = false, Errors = errors.ToList() };
}

