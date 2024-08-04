using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Core.Text;
using EzrSquared.Util.Extensions;
using System;
using System.Numerics;

namespace EzrSquared.Runtime.Types.Core.Numerics;

/// <summary>
/// The integer type object.
/// </summary>
/// <param name="value">The value.</param>
/// <param name="parentContext">The parent context.</param>
/// <param name="startPosition">The starting position of the object.</param>
/// <param name="endPosition">The ending position of the object.</param>
public class EzrInteger(BigInteger value, Context parentContext, Position startPosition, Position endPosition) : EzrObject(parentContext, startPosition, endPosition)
{
    /// <summary>
    /// Represents the size of the integer value stored in an <see cref="EzrInteger"/> object.
    /// </summary>
    public enum Size
    {
        /// <summary>
        /// Same as <see cref="int"/>.
        /// </summary>
        Int32,

        /// <summary>
        /// Same as <see cref="long"/>.
        /// </summary>
        Int64,

        /// <summary>
        /// Large, unknown. See also <see cref="BigInteger"/>.
        /// </summary>
        Big
    }

    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "integer";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.Integer";

    /// <summary>
    /// The integer value.
    /// </summary>
    public readonly BigInteger Value = value;

    /// <summary>
    /// The size of the <see cref="Value"/>.
    /// </summary>
    public readonly Size ValueSize = value switch
    {
        BigInteger bigInteger when bigInteger > int.MinValue && bigInteger < int.MaxValue => Size.Int32,
        BigInteger bigInteger when bigInteger > long.MinValue && bigInteger < long.MaxValue => Size.Int64,
        _ => Size.Big,
    };

    /// <summary>
    /// The double representation of <see cref="Value"/>. May be <see langword="null"/>.
    /// </summary>
    private double? _doubleRepresentation;

    /// <summary>
    /// The long representation of <see cref="Value"/>. May be <see langword="null"/>.
    /// </summary>
    private long? _longRepresentation;

    /// <summary>
    /// The int representation of <see cref="Value"/>. May be <see langword="null"/>.
    /// </summary>
    private int? _intRepresentation;

    /// <summary>
    /// Gets a double representation of the current object.
    /// </summary>
    /// <returns>The double representation.</returns>
    public double GetDoubleRepresentation()
    {
        return _doubleRepresentation is not null
            ? (double)_doubleRepresentation
            : (double)(_doubleRepresentation = (double)Value);
    }

    /// <summary>
    /// Tries to get a long representation of the current object.
    /// </summary>
    /// <param name="value">The long representation.</param>
    /// <returns><see langword="true"/> if successful, <see langword="false"/> otherwise.</returns>
    public bool TryGetLongRepresentation(out long value)
    {
        if (_longRepresentation is not null)
        {
            value = (long)_longRepresentation;
            return true;
        }

        switch (ValueSize)
        {
            case Size.Int32:
            case Size.Int64:
                _longRepresentation = value = (long)Value;
                return true;
            default:
                value = 0;
                return false;
        }
    }

    /// <summary>
    /// Tries to get an int representation of the current object.
    /// </summary>
    /// <param name="value">The int representation.</param>
    /// <returns><see langword="true"/> if successful, <see langword="false"/> otherwise.</returns>
    public bool TryGetIntRepresentation(out int value)
    {
        if (_intRepresentation is not null)
        {
            value = (int)_intRepresentation;
            return true;
        }

        if (ValueSize == Size.Int32)
        {
            _intRepresentation = value = (int)Value;
            return true;
        }

        value = 0;
        return false;
    }

    /// <summary>
    /// Gets an int representation of the current object. Use only if you know the value of the current object will fit in an int value.
    /// </summary>
    /// <returns>The int representation.</returns>
    public int GetIntRepresentation()
    {
        return _intRepresentation is not null
            ? (int)_intRepresentation
            : (int)(_intRepresentation = (int)Value);
    }

    /// <inheritdoc/>
    public override void ComparisonEqual(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger otherInteger:
                result.Success(NewBooleanConstant(Value == otherInteger.Value)); break;
            case EzrFloat otherFloat:
                result.Success(NewBooleanConstant(GetDoubleRepresentation() == otherFloat.Value)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(Value == otherCharacter.Value)); break;
            default:
                result.Success(NewBooleanConstant(false)); break;
        }
    }

    /// <inheritdoc/>
    public override void ComparisonNotEqual(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger otherInteger:
                result.Success(NewBooleanConstant(Value != otherInteger.Value)); break;
            case EzrFloat otherFloat:
                result.Success(NewBooleanConstant(GetDoubleRepresentation() != otherFloat.Value)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(Value != otherCharacter.Value)); break;
            default:
                result.Success(NewBooleanConstant(true)); break;
        }
    }

    /// <inheritdoc/>
    public override void ComparisonLessThan(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger otherInteger:
                result.Success(NewBooleanConstant(Value < otherInteger.Value)); break;
            case EzrFloat otherFloat:
                result.Success(NewBooleanConstant(GetDoubleRepresentation() < otherFloat.Value)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(Value < otherCharacter.Value)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void ComparisonGreaterThan(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger otherInteger:
                result.Success(NewBooleanConstant(Value > otherInteger.Value)); break;
            case EzrFloat otherFloat:
                result.Success(NewBooleanConstant(GetDoubleRepresentation() > otherFloat.Value)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(Value > otherCharacter.Value)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void ComparisonLessThanOrEqual(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger otherInteger:
                result.Success(NewBooleanConstant(Value <= otherInteger.Value)); break;
            case EzrFloat otherFloat:
                result.Success(NewBooleanConstant(GetDoubleRepresentation() <= otherFloat.Value)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(Value <= otherCharacter.Value)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void ComparisonGreaterThanOrEqual(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger otherInteger:
                result.Success(NewBooleanConstant(Value >= otherInteger.Value)); break;
            case EzrFloat otherFloat:
                result.Success(NewBooleanConstant(GetDoubleRepresentation() >= otherFloat.Value)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(Value >= otherCharacter.Value)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void Addition(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger otherInteger:
                result.Success(NewIntegerConstant(Value + otherInteger.Value)); break;
            case EzrFloat otherFloat:
                result.Success(NewFloatConstant(GetDoubleRepresentation() + otherFloat.Value)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewIntegerConstant(Value + otherCharacter.Value)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void Subtraction(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger otherInteger:
                result.Success(NewIntegerConstant(Value - otherInteger.Value)); break;
            case EzrFloat otherFloat:
                result.Success(NewFloatConstant(GetDoubleRepresentation() - otherFloat.Value)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewIntegerConstant(Value - otherCharacter.Value)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void Multiplication(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger otherInteger:
                result.Success(NewIntegerConstant(Value * otherInteger.Value)); break;
            case EzrFloat otherFloat:
                result.Success(NewFloatConstant(GetDoubleRepresentation() * otherFloat.Value)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewIntegerConstant(Value * otherCharacter.Value)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void Division(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger { Value: BigInteger value } when value == 0:
            case EzrFloat { Value: 0 }:
            case EzrCharacter { Value: '\0' }:
                result.Failure(new EzrMathError("Division error", "Divisor cannot be zero!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case EzrInteger otherInteger:
                result.Success(NewIntegerConstant(Value / otherInteger.Value)); break;
            case EzrFloat otherFloat:
                result.Success(NewFloatConstant(GetDoubleRepresentation() / otherFloat.Value)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewIntegerConstant(Value / otherCharacter.Value)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void Modulo(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger { Value: BigInteger value } when value == 0:
            case EzrFloat { Value: 0 }:
            case EzrCharacter { Value: '\0' }:
                result.Failure(new EzrMathError("Division error", "Divisor cannot be zero!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case EzrInteger otherInteger:
                result.Success(NewIntegerConstant(Value % otherInteger.Value)); break;
            case EzrFloat otherFloat:
                result.Success(NewFloatConstant(GetDoubleRepresentation() % otherFloat.Value)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewIntegerConstant(Value % otherCharacter.Value)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void Power(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger otherInteger when otherInteger.TryGetIntRepresentation(out int otherInteger32):
                result.Success(NewIntegerConstant(BigInteger.Pow(Value, otherInteger32))); break;
            case EzrInteger otherInteger:
                result.Success(NewIntegerConstant(Value.Power(otherInteger.Value))); break;
            case EzrCharacter otherCharacter:
                result.Success(NewIntegerConstant(BigInteger.Pow(Value, otherCharacter.Value))); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void Negation(RuntimeResult result)
    {
        result.Success(NewIntegerConstant(-Value));
    }

    /// <inheritdoc/>
    public override void Affirmation(RuntimeResult result)
    {
        result.Success(NewIntegerConstant(BigInteger.Abs(Value)));
    }

    /// <inheritdoc/>
    public override void BitwiseOr(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger otherInteger:
                result.Success(NewIntegerConstant(Value | otherInteger.Value)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewIntegerConstant(Value | otherCharacter.Value)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void BitwiseXOr(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger otherInteger:
                result.Success(NewIntegerConstant(Value ^ otherInteger.Value)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewIntegerConstant(Value ^ otherCharacter.Value)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void BitwiseAnd(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger otherInteger:
                result.Success(NewIntegerConstant(Value & otherInteger.Value)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewIntegerConstant(Value & otherCharacter.Value)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void BitwiseLeftShift(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger when ValueSize != Size.Int32:
            case EzrCharacter when ValueSize != Size.Int32:
                result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", _executionContext, StartPosition, EndPosition)); break;

            case EzrInteger otherInteger when otherInteger.ValueSize != Size.Int32:
                result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case EzrInteger otherInteger:
                result.Success(NewIntegerConstant(GetIntRepresentation() << otherInteger.GetIntRepresentation()));
                break;

            case EzrCharacter otherCharacter:
                result.Success(NewIntegerConstant(GetIntRepresentation() << otherCharacter.Value));
                break;

            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void BitwiseRightShift(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger when ValueSize != Size.Int32:
            case EzrCharacter when ValueSize != Size.Int32:
                result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", _executionContext, StartPosition, EndPosition)); break;

            case EzrInteger otherInteger when otherInteger.ValueSize != Size.Int32:
                result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case EzrInteger otherInteger:
                result.Success(NewIntegerConstant(GetIntRepresentation() >> otherInteger.GetIntRepresentation()));
                break;

            case EzrCharacter otherCharacter:
                result.Success(NewIntegerConstant(GetIntRepresentation() >> otherCharacter.Value));
                break;

            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void BitwiseNegation(RuntimeResult result)
    {
        result.Success(NewIntegerConstant(~Value));
    }

    /// <inheritdoc/>
    public override void Inversion(RuntimeResult result)
    {
        result.Success(NewIntegerConstant(Value > 0 ? 0 : 1));
    }

    /// <inheritdoc/>
    public override bool EvaluateBoolean(RuntimeResult result)
    {
        return Value > 0;
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return (other as EzrInteger)?.Value == Value && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return HashCode.Combine(HashTag, Value);
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return $"{Value}";
    }
}
