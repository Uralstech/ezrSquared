using System;

namespace EzrSquared.Runtime.Types.Core;

/// <summary>
/// The boolean type object.
/// </summary>
/// <param name="value">The base value.</param>
/// <param name="parentContext">The parent context.</param>
/// <param name="startPosition">The starting position of the object.</param>
/// <param name="endPosition">The ending position of the object.</param>
public class EzrBoolean(bool value, Context parentContext, Position startPosition, Position endPosition) : EzrObject(parentContext, startPosition, endPosition)
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "boolean";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.Boolean";

    /// <summary>
    /// The boolean value of the object.
    /// </summary>
    public readonly bool Value = value;

    /// <inheritdoc/>
    public override void ComparisonEqual(IEzrObject other, RuntimeResult result)
    {
        bool otherEvaluation = other.EvaluateBoolean(result);
        if (result.ShouldReturn)
            return;

        result.Success(NewBooleanConstant(otherEvaluation == Value));
    }

    /// <inheritdoc/>
    public override void ComparisonNotEqual(IEzrObject other, RuntimeResult result)
    {
        bool otherEvaluation = other.EvaluateBoolean(result);
        if (result.ShouldReturn)
            return;

        result.Success(NewBooleanConstant(otherEvaluation != Value));
    }

    /// <inheritdoc/>
    public override void Inversion(RuntimeResult result)
    {
        result.Success(NewBooleanConstant(!Value));
    }

    /// <inheritdoc/>
    public override bool EvaluateBoolean(RuntimeResult result)
    {
        return Value;
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return (other as EzrBoolean)?.Value == Value && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return HashCode.Combine(HashTag, Value);
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return Value ? "true" : "false";
    }
}
