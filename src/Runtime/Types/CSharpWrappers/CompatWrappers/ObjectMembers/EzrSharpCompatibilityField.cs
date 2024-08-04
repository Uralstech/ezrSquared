using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Util;
using System;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers;

public class EzrSharpCompatibilityField : EzrSharpCompatibilityWrapper
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp field";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpField";

    public readonly FieldInfo SharpField;
    public readonly object? Instance;
    public readonly string SharpFieldName;

    public EzrSharpCompatibilityField(FieldInfo sharpField, object? instance, Context parentContext, Position startPosition, Position endPosition) : base(parentContext, startPosition, endPosition)
    {
        SharpField = sharpField;
        Instance = instance;
        SharpFieldName = Utils.PascalToSnakeCase(SharpField.Name);
        Tag = $"{Tag}.{SharpFieldName}.{Utils.GetNextUniqueId()}";
    }

    public override void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        if (arguments.Length > 1)
        {
            result.Failure(new EzrUnexpectedArgumentError($"Only expected 0 (for getting the value) or 1 (for setting the value) argument(s) for CSharp field wrapper \"{SharpFieldName}\"!", Context, StartPosition, EndPosition));
            return;
        }

        if (arguments.Length == 0)
        {
            object? value = SharpField.GetValue(Instance);
            PrimitiveToEzrObject(value, Type.GetTypeCode(value?.GetType()), result);
        }
        else
        {
            object? value = EzrObjectToPrimitive(arguments[0].Object, Type.GetTypeCode(SharpField.FieldType), result);
            if (result.ShouldReturn)
                return;

            try
            {
                SharpField.SetValue(Instance, value);
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
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return other is EzrSharpCompatibilityField field
            && field.SharpField == SharpField
            && field.Instance?.GetHashCode() == Instance?.GetHashCode()
            && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return HashCode.Combine(HashTag, SharpField);
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return $"<{TypeName} \"{SharpFieldName}\">";
    }
}
