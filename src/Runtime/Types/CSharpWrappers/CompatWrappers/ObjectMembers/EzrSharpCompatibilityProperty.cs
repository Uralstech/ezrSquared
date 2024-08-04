using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Util;
using System;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers;

public class EzrSharpCompatibilityProperty : EzrSharpCompatibilityWrapper
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp property";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpProperty";

    public readonly PropertyInfo SharpProperty;
    public readonly object? Instance;
    public readonly string SharpPropertyName;

    public EzrSharpCompatibilityProperty(PropertyInfo sharpProperty, object? instance, Context parentContext, Position startPosition, Position endPosition) : base(parentContext, startPosition, endPosition)
    {
        SharpProperty = sharpProperty;
        Instance = instance;
        SharpPropertyName = Utils.PascalToSnakeCase(SharpProperty.Name);
        Tag = $"{Tag}.{SharpPropertyName}.{Utils.GetNextUniqueId()}";
    }

    public override void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        if (arguments.Length > 1)
        {
            result.Failure(new EzrUnexpectedArgumentError($"Only expected 0 (for getting the value) or 1 (for setting the value) argument(s) for CSharp property wrapper \"{SharpPropertyName}\"!", Context, StartPosition, EndPosition));
            return;
        }

        if (arguments.Length == 0)
        {
            object? value = SharpProperty.GetValue(Instance);
            PrimitiveToEzrObject(value, Type.GetTypeCode(value?.GetType()), result);
        }
        else
        {
            object? value = EzrObjectToPrimitive(arguments[0].Object, Type.GetTypeCode(SharpProperty.PropertyType), result);
            if (result.ShouldReturn)
                return;

            if (!SharpProperty.CanWrite || SharpProperty.SetMethod is null)
            {
                result.Failure(new EzrIllegalOperationError($"Cannot set value to CSharp property wrapper \"{SharpPropertyName}\" as it is read-only!", Context, StartPosition, EndPosition));
                return;
            }

            if (!SharpProperty.SetMethod.IsPublic)
            {
                result.Failure(new EzrIllegalOperationError($"Cannot set value to CSharp property wrapper \"{SharpPropertyName}\" as it does not have a public setter method!", Context, StartPosition, EndPosition));
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
        return $"<{TypeName} \"{SharpPropertyName}\">";
    }
}
