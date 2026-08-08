namespace Inventory.Shared.Common;

/// <summary>
/// Represents the outcome of an operation, carrying either a success value or error details.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; private init; }
    public T? Value { get; private init; }
    public string? Error { get; private init; }

    private Result() { }

    public static Result<T> Success(T value) =>
        new() { IsSuccess = true, Value = value };

    public static Result<T> Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}

/// <summary>
/// Non-generic Result for operations that do not return a value.
/// </summary>
public class Result
{
    public bool IsSuccess { get; private init; }
    public string? Error { get; private init; }

    private Result() { }

    public static Result Success() => new() { IsSuccess = true };

    public static Result Failure(string error) => new() { IsSuccess = false, Error = error };
}
