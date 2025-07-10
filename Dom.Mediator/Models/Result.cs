namespace Dom.Mediator.ResultPattern;

public record Error(string Code, string Description, ErrorType Type = ErrorType.Unknown);

public class Result<T>
{
    public bool IsSuccess { get; }
    public List<Error> Errors { get; }

    public T? Value { get; }

    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, T? value, List<Error> errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
    }

    public static Result<T> Success(T value) => new(true, value, new List<Error>());

    public static Result<T> Failure(string errorCode, string description, ErrorType type = ErrorType.Unknown)
    { 
        List<Error> errors = new List<Error> { new(errorCode, description, type) };

        return new Result<T>(false, default, errors);
    }

    public static Result<T> Failure(List<Error> errors)
    {
        return new Result<T>(false, default, errors);
    }
}

/// <summary>
/// Represents a result without a value payload, used for operations that don't return data
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public List<Error> Errors { get; }
    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, List<Error> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public static Result Success() => new(true, new List<Error>());

    public static Result Failure(string errorCode, string description, ErrorType type = ErrorType.Unknown)
    { 
        List<Error> errors = new List<Error> { new(errorCode, description, type) };
        return new Result(false, errors);
    }

    public static Result Failure(List<Error> errors)
    {
        return new Result(false, errors);
    }
}
