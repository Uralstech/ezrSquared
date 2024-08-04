using EzrSquared.Runtime.Types.Core;
using EzrSquared.Runtime.WrapperAttributes;
using EzrSquared.Util;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.SourceWrappers;
public class EzrSharpSourcePropertyWrapper : EzrSharpSourceExecutableWrapper
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp source field wrapper";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpSourceFieldWrapper";

    public readonly object? SharpInstance;
    public readonly PropertyInfo SharpProperty;
    public readonly string SharpPropertyName;

    public EzrSharpSourcePropertyWrapper(PropertyInfo propertyInfo, object? instance, Context parentContext, Position startPosition, Position endPosition) : base(parentContext, startPosition, endPosition)
    {
        SharpInstance = instance;
        SharpProperty = propertyInfo;

        SharpFieldWrapperAttribute attribute = SharpProperty.GetCustomAttribute<SharpFieldWrapperAttribute>(true) ?? throw new ArgumentException($"No \"{nameof(SharpFieldWrapperAttribute)}\" attribute found!", nameof(propertyInfo));

        Exception? propertyTypeException = SharpFieldWrapperAttribute.ValidateProperty(SharpProperty);
        if (propertyTypeException is not null)
            throw propertyTypeException;

        SharpPropertyName = attribute.Name;

        if (SharpProperty.CanWrite && !attribute.IsReadOnly)
            Parameters = [("value", false)];

        Tag = $"{Tag}.{SharpPropertyName}.{Utils.GetNextUniqueId()}";
    }

    public override void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        Dictionary<string, Reference> argumentReferences = CheckAndPopulateArguments(arguments, result);
        if (result.ShouldReturn)
            return;

        if (argumentReferences.TryGetValue("value", out Reference? newValue))
            SharpProperty.SetValue(SharpInstance, newValue.Object);

        IEzrObject value = (IEzrObject?)SharpProperty.GetValue(SharpInstance) ?? EzrConstants.Nothing;
        result.Success(ReferencePool.Get(value, AccessMod.PrivateConstant));
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return HashCode.Combine(HashTag, SharpProperty);
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return (other as EzrSharpSourceFieldWrapper)?.SharpField == SharpProperty && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return $"<{TypeName} \"{SharpPropertyName}\">";
    }
}
