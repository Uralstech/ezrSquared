using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Util;
using System;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers;

/// <summary>
/// Class to automatically wrap C# properties so that they can be used in ezr².
/// </summary>
public class EzrSharpCompatibilityProperty : EzrSharpCompatibilityWrapper
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp property";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpProperty";

    /// <summary>
    /// Reflection information about the wrapped property.
    /// </summary>
    public readonly PropertyInfo SharpProperty;

    /// <summary>
    /// The object which contains the property, <see langword="null"/> if static.
    /// </summary>
    public readonly object? Instance;

    /// <summary>
    /// Creates a new <see cref="EzrSharpCompatibilityProperty"/>.
    /// </summary>
    /// <param name="sharpProperty">The property to wrap.</param>
    /// <param name="instance">The object which contains the property, <see langword="null"/> if static.</param>
    /// <param name="parentContext">The context in which this object was created.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    /// <param name="skipValidation">Skip property type validation?</param>
    public EzrSharpCompatibilityProperty(PropertyInfo sharpProperty, object? instance, Context parentContext, Position startPosition, Position endPosition, bool skipValidation = false) : base(sharpProperty, parentContext, startPosition, endPosition)
    {
        SharpProperty = sharpProperty;
        Instance = instance;
        Tag = $"{Tag}.{SharpMemberName}.{Utils.GetNextUniqueId()}";

        if (!skipValidation)
            Validate();
    }

    /// <summary>
    /// If there are no arguments, accesses the property's value. If the property is not read-only and there is an arguments, sets the property's value.
    /// </summary>
    /// <inheritdoc/>
    public override void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        if (arguments.Length > 1)
        {
            result.Failure(new EzrUnexpectedArgumentError($"Only expected 0 (for getting the value) or 1 (for setting the value) argument(s) for CSharp property wrapper \"{SharpMemberName}\"!", Context, StartPosition, EndPosition));
            return;
        }

        if (arguments.Length == 0)
        {
            if (AutoWrapperAttribute?.IsWriteOnly == true || SharpProperty.GetMethod is null || (AutoWrapperAttribute == null && !SharpProperty.GetMethod.IsPublic))
            {
                result.Failure(new EzrIllegalOperationError($"Cannot get value from CSharp property wrapper \"{SharpMemberName}\" as it is write-only!", Context, StartPosition, EndPosition));
                return;
            }

            object? value = SharpProperty.GetValue(Instance);
            CSharpToEzrObject(value, result);
        }
        else
        {
            object? value = EzrObjectToCSharp(arguments[0].Object, SharpProperty.PropertyType, result);
            if (result.ShouldReturn)
                return;

            if (AutoWrapperAttribute?.IsReadOnly == true || SharpProperty.SetMethod is null || (AutoWrapperAttribute == null && !SharpProperty.SetMethod.IsPublic))
            {
                result.Failure(new EzrIllegalOperationError($"Cannot set value to CSharp property wrapper \"{SharpMemberName}\" as it is read-only!", Context, StartPosition, EndPosition));
                return;
            }

            try
            {
                SharpProperty.SetValue(Instance, value);
                result.Success(NewNothingConstant());
            }
            catch (Exception error)
            {
                result.Failure(new EzrWrapperExecutionError(error.Message, Context, StartPosition, EndPosition));
                return;
            }
        }
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return HashCode.Combine(HashTag, SharpProperty);
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return other is EzrSharpCompatibilityProperty property
            && property.SharpProperty == SharpProperty
            && property.Instance?.GetHashCode() == Instance?.GetHashCode()
            && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return $"<{TypeName} \"{SharpMemberName}\">";
    }
}
