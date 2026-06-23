namespace RMS.BuildingBlocks.Results;

/// <summary>
/// Represents the outcome of an operation without relying on exceptions
/// for expected/business-rule failures.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public string? ErrorCode { get; }

    protected Result(bool isSuccess, string? error, string? errorCode)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("A successful result cannot contain an error.");
        if (!isSuccess && error is null)
            throw new InvalidOperationException("A failed result must contain an error.");

        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string error, string? errorCode = null) => new(false, error, errorCode);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, null, null);
    public static Result<TValue> Failure<TValue>(string error, string? errorCode = null) =>
        new(default, false, error, errorCode);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, string? error, string? errorCode)
        : base(isSuccess, error, errorCode)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static implicit operator Result<TValue>(TValue value) => Success(value);
}
