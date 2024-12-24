using System;

namespace EzrSquared.Runtime.Types.Core;

/// <summary>
/// The <see langword="null"/>-equivalent type.
/// </summary>
/// <param name="parentContext">The parent context.</param>
/// <param name="startPosition">The starting position of the object.</param>
/// <param name="endPosition">The ending position of the object.</param>
public class EzrNothing(Context parentContext, Position startPosition, Position endPosition) : EzrObject(parentContext, startPosition, endPosition)
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "nothing";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.Nothing";

    /// <inheritdoc/>
    public override void ComparisonEqual(IEzrObject other, RuntimeResult result)
    {
        result.Success(NewBooleanConstant(other is EzrNothing));
    }

    /// <inheritdoc/>
    public override void ComparisonNotEqual(IEzrObject other, RuntimeResult result)
    {
        result.Success(NewBooleanConstant(other is not EzrNothing));
    }

    /// <inheritdoc/>
    public override bool EvaluateBoolean(RuntimeResult result)
    {
        return false;
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return HashCode.Combine(HashTag, 0);
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return "nothing";
    }
}
