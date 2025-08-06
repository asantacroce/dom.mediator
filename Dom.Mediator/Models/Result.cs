namespace Dom.Mediator;

public class Result<T>
{
    public bool IsSuccess { get; }
    public Error? Error { get; }

    public T? Value { get; }

    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, T? value, Error? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Failure(
        string errorCode, 
        string description, 
        string type)
    { 
        Error error = new Error(errorCode, description, type);

        return new Result<T>(false, default, error);
    }

    public static Result<T> Failure(
        Error error)
    {
        return new Result<T>(false, default, error);
    }
}

/// <summary>
/// Represents a result without a value payload, used for operations that don't return data
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }
    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);

    public static Result Failure(
        string errorCode, 
        string description, 
        string type)
    { 
        Error error = new(errorCode, description, type);
        return new Result(false, error);
    }

    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }
}
