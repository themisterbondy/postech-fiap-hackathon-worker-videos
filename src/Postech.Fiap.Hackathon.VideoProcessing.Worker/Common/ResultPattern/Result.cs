namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Common.ResultPattern;

/// <summary>
///     Represents a result of some operation, with status information and possibly an error.
/// </summary>
[ExcludeFromCodeCoverage]
public class Result
{
    /// <summary>
    ///     Represents the result of an operation.
    /// </summary>
    /// <param name="isSuccess">A boolean value indicating whether the operation was successful.</param>
    /// <param name="error">An instance of the Error class representing the error that occurred during the operation, if any.</param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the isSuccess parameter is true and the error parameter is not Error.None,
    ///     or when the isSuccess parameter is false and the error parameter is Error.None.
    /// </exception>
    protected Result(bool isSuccess, Error error)
    {
        switch (isSuccess)
        {
            case true when error != Error.None:
                throw new InvalidOperationException();
            case false when error == Error.None:
                throw new InvalidOperationException();
            default:
                IsSuccess = isSuccess;
                Error = error;
                break;
        }
    }

    protected Result(bool isSuccess, Error[] errors)
    {
        switch (isSuccess)
        {
            case true when errors != null:
                throw new InvalidOperationException();
            case false when errors == null:
                throw new InvalidOperationException();
            default:
                IsSuccess = isSuccess;
                Errors = errors;
                break;
        }
    }

    protected Result(bool isSuccess, Error error, Error[] errors)
    {
        switch (isSuccess)
        {
            case true when error != Error.None:
                throw new InvalidOperationException();
            case false when error == Error.None:
                throw new InvalidOperationException();
            default:
                IsSuccess = isSuccess;
                Error = error;
                Errors = errors;
                break;
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the operation was successful or not.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the operation was successful; otherwise, <c>false</c>.
    /// </value>
    public bool IsSuccess { get; }

    /// <summary>
    ///     Gets a value indicating whether the operation is a failure.
    /// </summary>
    /// <remarks>
    ///     This property returns the negation of the <see cref="IsSuccess" /> property.
    /// </remarks>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    ///     Gets the error property.
    /// </summary>
    /// <value>
    ///     The error.
    /// </value>
    public Error? Error { get; }

    public Error[]? Errors { get; }

    /// <summary>
    ///     Creates a new instance of the Result class with a successful result and no error.
    /// </summary>
    /// <returns>A new instance of the Result class with a successful result and no error.</returns>
    public static Result Success()
    {
        return new Result(true, Error.None);
    }

    /// <summary>
    ///     Creates a successful result with the specified value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The value to be set.</param>
    /// <returns>A new instance of the result class with the specified value set as the result.</returns>
    public static Result<TValue> Success<TValue>(TValue value)
    {
        return new Result<TValue>(value, true, Error.None);
    }

    /// <summary>
    ///     Creates a failure result with the specified error.
    /// </summary>
    /// <param name="error">The error to associate with the failure result.</param>
    /// <returns>A new <see cref="Result" /> object representing a failure with the specified error.</returns>
    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }

    public static Result Failure(Error[] errors)
    {
        return new Result(false, errors);
    }

    public static Result Failure(Error error, Error[] errors)
    {
        return new Result(false, error, errors);
    }

    /// <summary>
    ///     Returns a failure result with the specified error.
    /// </summary>
    /// <param name="error">The error associated with the failure.</param>
    /// <typeparam name="TValue">The type of the value in the result.</typeparam>
    /// <returns>A Result object representing a failure with the specified error.</returns>
    public static Result<TValue> Failure<TValue>(Error error)
    {
        return new Result<TValue>(default, false, error);
    }

    public static Result<TValue> Failure<TValue>(Error[] errors)
    {
        return new Result<TValue>(default, false, errors);
    }

    public static Result<TValue> Failure<TValue>(Error error, Error[] errors)
    {
        return new Result<TValue>(default, false, error, errors);
    }

    /// <summary>
    ///     Creates a new instance of the Result class with the specified value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The value to wrap in the Result instance.</param>
    /// <returns>
    ///     A Result instance containing the specified value if it is not null, otherwise a Result instance indicating a
    ///     failure with the Error.NullValue.
    /// </returns>
    public static Result<TValue> Create<TValue>(TValue? value)
    {
        return value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
    }
}