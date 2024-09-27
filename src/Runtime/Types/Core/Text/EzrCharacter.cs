using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Core.Numerics;
using EzrSquared.Util.Extensions;
using System;
using System.Numerics;

namespace EzrSquared.Runtime.Types.Core.Text;

/// <summary>
/// The character type object.
/// </summary>
/// <param name="value">The base value.</param>
/// <param name="parentContext">The parent context.</param>
/// <param name="startPosition">The starting position of the object.</param>
/// <param name="endPosition">The ending position of the object.</param>
public class EzrCharacter(char value, Context parentContext, Position startPosition, Position endPosition) : EzrObject(parentContext, startPosition, endPosition), IEzrString
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "character";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.Character";

    /// <summary>
    /// The character value.
    /// </summary>
    public readonly char Value = value;

    /// <inheritdoc/>
    public string StringValue => Value.ToString();

    /// <inheritdoc/>
    public override void ComparisonEqual(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(Value == otherCharacter.Value)); break;
            case EzrInteger otherInteger:
                result.Success(NewBooleanConstant(Value == otherInteger.Value)); break;
            case EzrFloat otherFloat:
                result.Success(NewBooleanConstant(Value == otherFloat.Value)); break;
            case IEzrString otherString:
                result.Success(NewBooleanConstant(StringValue == otherString.StringValue)); break;
            default:
                result.Success(NewBooleanConstant(false)); break;
        }
    }

    /// <inheritdoc/>
    public override void ComparisonNotEqual(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(Value != otherCharacter.Value)); break;
            case EzrInteger otherInteger:
                result.Success(NewBooleanConstant(Value != otherInteger.Value)); break;
            case EzrFloat otherFloat:
                result.Success(NewBooleanConstant(Value != otherFloat.Value)); break;
            case IEzrString otherString:
                result.Success(NewBooleanConstant(StringValue != otherString.StringValue)); break;
            default:
                result.Success(NewBooleanConstant(true)); break;
        }
    }

    /// <inheritdoc/>
    public override void ComparisonLessThan(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(Value < otherCharacter.Value)); break;
            case EzrInteger otherInteger:
                result.Success(NewBooleanConstant(Value < otherInteger.Value)); break;
            case EzrFloat otherFloat:
                result.Success(NewBooleanConstant(Value < otherFloat.Value)); break;
            case IEzrString otherString:
                result.Success(NewBooleanConstant(Value.CompareTo(otherString.StringValue) < 0)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void ComparisonGreaterThan(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(Value > otherCharacter.Value)); break;
            case EzrInteger otherInteger:
                result.Success(NewBooleanConstant(Value > otherInteger.Value)); break;
            case EzrFloat otherFloat:
                result.Success(NewBooleanConstant(Value > otherFloat.Value)); break;
            case IEzrString otherString:
                result.Success(NewBooleanConstant(Value.CompareTo(otherString.StringValue) > 0)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void ComparisonLessThanOrEqual(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(Value <= otherCharacter.Value)); break;
            case EzrInteger otherInteger:
                result.Success(NewBooleanConstant(Value <= otherInteger.Value)); break;
            case EzrFloat otherFloat:
                result.Success(NewBooleanConstant(Value <= otherFloat.Value)); break;
            case IEzrString otherString:
                result.Success(NewBooleanConstant(Value.CompareTo(otherString.StringValue) <= 0)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void ComparisonGreaterThanOrEqual(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(Value >= otherCharacter.Value)); break;
            case EzrInteger otherInteger:
                result.Success(NewBooleanConstant(Value >= otherInteger.Value)); break;
            case EzrFloat otherFloat:
                result.Success(NewBooleanConstant(Value >= otherFloat.Value)); break;
            case IEzrString otherString:
                result.Success(NewBooleanConstant(Value.CompareTo(otherString.StringValue) >= 0)); break;
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
                result.Success(NewFloatConstant(Value + otherFloat.Value)); break;
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
                result.Success(NewFloatConstant(Value - otherFloat.Value)); break;
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
                result.Success(NewFloatConstant(Value * otherFloat.Value)); break;
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
                result.Success(NewFloatConstant(Value / otherFloat.Value)); break;
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
                result.Success(NewFloatConstant(Value % otherFloat.Value)); break;
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
        result.Success(NewIntegerConstant(Math.Abs(Value)));
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
        return (other as EzrCharacter)?.Value == Value && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return HashCode.Combine(HashTag, Value);
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return $"`{Value}`";
    }

    /// <inheritdoc/>
    public override string ToPureString(RuntimeResult result)
    {
        return $"{Value}";
    }
}
