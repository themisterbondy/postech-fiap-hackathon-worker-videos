namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Common.ResultPattern;

/// <summary>
/// Represents an error with its code, message, and type.
/// </summary>
[ExcludeFromCodeCoverage]
public record Error
{
    /// <summary>
    /// Represents a special error instance used to represent no error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    /// <summary>
    /// Represents a special error instance used to represent no error.
    /// </summary>
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.",
        ErrorType.Failure);

    private Error(string code, string message, ErrorType errorType)
    {
        Code = code;
        Message = message;
        ErrorType = errorType;
    }

    public string Code { get; }
    public string Message { get; }
    public ErrorType ErrorType { get; }

    /// <summary>
    /// Represents a failure error with its code and message.
    /// </summary>
    /// <param name="code">The code of the error.</param>
    /// <param name="message">The message describing the error.</param>
    /// <returns>A new instance of the Error class representing a failure error.</returns>
    public static Error Failure(string code, string message)
    {
        return new Error(code, message, ErrorType.Failure);
    }

    /// <summary>
    ///     Represents a validation error with its code and message.
    /// </summary>
    /// <param name="code">The code of the error.</param>
    /// <param name="message">The message describing the error.</param>
    /// <returns>A new instance of the Error class representing a validation error.</returns>
    public static Error Validation(string code, string message)
    {
        return new Error(code, message, ErrorType.Validation);
    }

    /// <summary>
    ///     Represents a not found error with its code and message.
    /// </summary>
    /// <param name="code">The code of the error.</param>
    /// <param name="message">The message describing the error.</param>
    /// <returns>A new instance of the Error class representing a not found error.</returns>
    public static Error NotFound(string code, string message)
    {
        return new Error(code, message, ErrorType.NotFound);
    }

    /// <summary>
    ///     Represents a conflict error with its code and message.
    /// </summary>
    /// <param name="code">The code of the error.</param>
    /// <param name="message">The message describing the error.</param>
    /// <returns>A new instance of the Error class representing a conflict error.</returns>
    public static Error Conflict(string code, string message)
    {
        return new Error(code, message, ErrorType.Conflict);
    }
}

/// <summary>
///     Enumeration of error types.
/// </summary>
public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3
}