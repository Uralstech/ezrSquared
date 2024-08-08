using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Util;
using System;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers;

/// <summary>
/// Class to automatically wrap C# fields so that they can be used in ezr².
/// </summary>
public class EzrSharpCompatibilityField : EzrSharpCompatibilityWrapper
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp field";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpField";

    /// <summary>
    /// Reflection information about the wrapped field.
    /// </summary>
    public readonly FieldInfo SharpField;

    /// <summary>
    /// The object which contains the field, <see langword="null"/> if static.
    /// </summary>
    public readonly object? Instance;

    /// <summary>
    /// The name of the field to wrap, in ezr² format (snake_case).
    /// </summary>
    public readonly string SharpFieldName;

    /// <summary>
    /// Creates a new <see cref="EzrSharpCompatibilityField"/>.
    /// </summary>
    /// <param name="name">The name of the field to wrap, in ezr² format (snake_case).</param>
    /// <param name="sharpField">The field to wrap.</param>
    /// <param name="instance">The object which contains the field, <see langword="null"/> if static.</param>
    /// <param name="parentContext">The context in which this object was created.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrSharpCompatibilityField(string name, FieldInfo sharpField, object? instance, Context parentContext, Position startPosition, Position endPosition) : base(parentContext, startPosition, endPosition)
    {
        SharpField = sharpField;
        Instance = instance;
        SharpFieldName = name;
        Tag = $"{Tag}.{SharpFieldName}.{Utils.GetNextUniqueId()}";
    }

    /// <summary>
    /// Creates a new <see cref="EzrSharpCompatibilityField"/>. Infers the name by converting the member's name to snake_case.
    /// </summary>
    /// <param name="sharpField">The field to wrap.</param>
    /// <param name="instance">The object which contains the field, <see langword="null"/> if static.</param>
    /// <param name="parentContext">The context in which this object was created.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrSharpCompatibilityField(FieldInfo sharpField, object? instance, Context parentContext, Position startPosition, Position endPosition)
        : this(Utils.PascalToSnakeCase(sharpField.Name), sharpField, instance, parentContext, startPosition, endPosition) { }

    /// <summary>
    /// If there are no arguments, accesses the field's value. If the field is not read-only and there is an arguments, sets the field's value.
    /// </summary>
    /// <inheritdoc/>
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
