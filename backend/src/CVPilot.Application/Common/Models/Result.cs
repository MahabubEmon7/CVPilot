namespace CVPilot.Application.Common.Models;

public class Result<T>
{
    public bool Succeeded { get; }
    public T? Data { get; }
    public string? Message { get; }
    public List<string> Errors { get; } = new();

    private Result(bool succeeded, T? data, string? message, IEnumerable<string>? errors = null)
    {
        Succeeded = succeeded;
        Data = data;
        Message = message;
        if (errors != null)
        {
            Errors.AddRange(errors);
        }
    }

    public static Result<T> Success(T data, string? message = null) =>
        new(true, data, message);

    public static Result<T> Failure(string error) =>
        new(false, default, null, new[] { error });

    public static Result<T> Failure(IEnumerable<string> errors) =>
        new(false, default, null, errors);
}

public class Result
{
    public bool Succeeded { get; }
    public string? Message { get; }
    public List<string> Errors { get; } = new();

    private Result(bool succeeded, string? message, IEnumerable<string>? errors = null)
    {
        Succeeded = succeeded;
        Message = message;
        if (errors != null)
        {
            Errors.AddRange(errors);
        }
    }

    public static Result Success(string? message = null) =>
        new(true, message);

    public static Result Failure(string error) =>
        new(false, null, new[] { error });

    public static Result Failure(IEnumerable<string> errors) =>
        new(false, null, errors);
}

