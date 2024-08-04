using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Core.Text;
using System;
using System.Numerics;

namespace EzrSquared.Runtime.Types.Core.Numerics;

/// <summary>
/// The float (actually double) type object.
/// </summary>
/// <param name="value">The base value.</param>
/// <param name="parentContext">The parent context.</param>
/// <param name="startPosition">The starting position of the object.</param>
/// <param name="endPosition">The ending position of the object.</param>
public class EzrFloat(double value, Context parentContext, Position startPosition, Position endPosition) : EzrObject(parentContext, startPosition, endPosition)
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "float";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.Float";

    /// <summary>
    /// The double value.
    /// </summary>
    public readonly double Value = value;

    /// <inheritdoc/>
    public override void ComparisonEqual(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger otherInteger:
                result.Success(NewBooleanConstant(Value == otherInteger.GetDoubleRepresentation())); break;
            case EzrFloat otherFloat:
                result.Success(NewBooleanConstant(Value == otherFloat.Value)); break;
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
                result.Success(NewBooleanConstant(Value != otherInteger.GetDoubleRepresentation())); break;
            case EzrFloat otherFloat:
                result.Success(NewBooleanConstant(Value != otherFloat.Value)); break;
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
                result.Success(NewBooleanConstant(Value < otherInteger.GetDoubleRepresentation())); break;
            case EzrFloat otherFloat:
                result.Success(NewBooleanConstant(Value < otherFloat.Value)); break;
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
                result.Success(NewBooleanConstant(Value > otherInteger.GetDoubleRepresentation())); break;
            case EzrFloat otherFloat:
                result.Success(NewBooleanConstant(Value > otherFloat.Value)); break;
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
                result.Success(NewBooleanConstant(Value <= otherInteger.GetDoubleRepresentation())); break;
            case EzrFloat otherFloat:
                result.Success(NewBooleanConstant(Value <= otherFloat.Value)); break;
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
                result.Success(NewBooleanConstant(Value >= otherInteger.GetDoubleRepresentation())); break;
            case EzrFloat otherFloat:
                result.Success(NewBooleanConstant(Value >= otherFloat.Value)); break;
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
                result.Success(NewFloatConstant(Value + otherInteger.GetDoubleRepresentation())); break;
            case EzrFloat otherFloat:
                result.Success(NewFloatConstant(Value + otherFloat.Value)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewFloatConstant(Value + otherCharacter.Value)); break;
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
                result.Success(NewFloatConstant(Value - otherInteger.GetDoubleRepresentation())); break;
            case EzrFloat otherFloat:
                result.Success(NewFloatConstant(Value - otherFloat.Value)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewFloatConstant(Value - otherCharacter.Value)); break;
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
                result.Success(NewFloatConstant(Value * otherInteger.GetDoubleRepresentation())); break;
            case EzrFloat otherFloat:
                result.Success(NewFloatConstant(Value * otherFloat.Value)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewFloatConstant(Value * otherCharacter.Value)); break;
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
                result.Success(NewFloatConstant(Value / otherInteger.GetDoubleRepresentation())); break;
            case EzrFloat otherFloat:
                result.Success(NewFloatConstant(Value / otherFloat.Value)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewFloatConstant(Value / otherCharacter.Value)); break;
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
                result.Success(NewFloatConstant(Value % otherInteger.GetDoubleRepresentation())); break;
            case EzrFloat otherFloat:
                result.Success(NewFloatConstant(Value % otherFloat.Value)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewFloatConstant(Value % otherCharacter.Value)); break;
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
                result.Success(NewFloatConstant(Math.Pow(Value, otherInteger.GetDoubleRepresentation()))); break;
            case EzrCharacter otherCharacter:
                result.Success(NewFloatConstant(Math.Pow(Value, otherCharacter.Value))); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void Negation(RuntimeResult result)
    {
        result.Success(NewFloatConstant(-Value));
    }

    /// <inheritdoc/>
    public override void Affirmation(RuntimeResult result)
    {
        result.Success(NewFloatConstant(Math.Abs(Value)));
    }

    /// <inheritdoc/>
    public override void Inversion(RuntimeResult result)
    {
        result.Success(NewFloatConstant(Value > 0 ? 0 : 1));
    }

    /// <inheritdoc/>
    public override bool EvaluateBoolean(RuntimeResult result)
    {
        return Value > 0f;
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return (other as EzrFloat)?.Value == Value && other.HashTag == HashTag;
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
