namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Common.ResultPattern;

/// <summary>
///     Represents a result that can contain a value of type TValue.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
[ExcludeFromCodeCoverage]
public class Result<TValue> : Result
{
    /// <summary>
    ///     Represents the value of a variable.
    /// </summary>
    private readonly TValue? _value;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Result{TValue}" /> class.
    /// </summary>
    /// <param name="value">The value of the result.</param>
    /// <param name="isSuccess">A flag indicating whether the result is a success.</param>
    /// <param name="error">The error associated with the result, if any.</param>
    protected internal Result(TValue? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    public Result(TValue? value, bool isSuccess, Error[] errors) : base(isSuccess, errors)
    {
        _value = value;
    }

    public Result(TValue? value, bool isSuccess, Error error, Error[] errors) : base(isSuccess, error, errors)
    {
        _value = value;
    }

    /// <summary>
    ///     Gets the value of the property.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when trying to access the value of a failure result.</exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    /// <summary>
    ///     Represents an implicit conversion operator for converting a nullable value to a Result object.
    /// </summary>
    /// <param name="value">The nullable value to be converted.</param>
    /// <typeparam name="TValue">The type of the nullable value.</typeparam>
    /// <returns>A Result object that represents the converted nullable value.</returns>
    public static implicit operator Result<TValue>(TValue? value)
    {
        return Create(value);
    }
}