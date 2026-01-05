namespace Core.Dto.Common;

public sealed class ResponseResult<T>
{
    public bool IsSuccess => Error == null;
    public T? Value { get; }
    public (ErrorCode Code, string Message)? Error { get; }

    // Private constructors force the use of static Factory Methods
    private ResponseResult(T value) => Value = value;
    
    private ResponseResult(ErrorCode code, string message) 
        => Error = (code, message);

    // Factory Methods
    public static ResponseResult<T> Success(T value) => new(value);

    public static ResponseResult<T> Failure(ErrorCode code, string? message = null)
    {
        string errorMessage = message ?? GetDefaultMessage(code);
        return new ResponseResult<T>(code, errorMessage);
    }

    private static string GetDefaultMessage(ErrorCode code) => code switch
    {
        ErrorCode.NotFound => "The requested resource was not found.",
        ErrorCode.AlreadyExists => "The resource already exists.",
        ErrorCode.InvalidState => "The operation cannot be performed in the current state.",
        ErrorCode.Unauthorized => "Authentication is required.",
        ErrorCode.Forbidden => "Permission denied.",
        ErrorCode.Conflict => "A conflict occurred.",
        _ => "An unexpected error occurred."
    };
}