using System;
using System.Collections.Generic;
using EzrSquared.Runtime.Nodes;
using EzrSquared.Runtime.Types.Core.Errors;

namespace EzrSquared.Runtime.Types;

/// <summary>
/// The "type" type object? You know.
/// </summary>
public class EzrClass : EzrRuntimeExecutable, IEzrMutableObject
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "class";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.Class";

    /// <summary>
    /// The parents of the class.
    /// </summary>
    public readonly EzrClass[] Parents;

    /// <summary>
    /// Is the class static?
    /// </summary>
    public readonly bool IsStatic;

    /// <summary>
    /// The references to the class's static parents.
    /// </summary>
    public readonly Reference[] StaticParentReferences;

    /// <summary>
    /// Creates a new class.
    /// </summary>
    /// <param name="name">The name of the executable.</param>
    /// <param name="body">The source code body of the executable.</param>
    /// <param name="parents">The parents of the class.</param>
    /// <param name="isReadOnly">Is this a read-only class?</param>
    /// <param name="isStatic">Is this a static class?</param>
    /// <param name="interpreter">Interpreter for executing the static body of the class.</param>
    /// <param name="result">Runtime result for carrying errors.</param>
    /// <param name="parentContext">The parent context.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrClass(string? name, Node body, EzrClass[] parents, bool isReadOnly, bool isStatic, Interpreter interpreter, RuntimeResult result, Context parentContext, Position startPosition, Position endPosition) : base(name, body, [], null, parentContext, startPosition, endPosition, new Context($"<{$"\"{name}\"" ?? "<anonymous>"} static context>", true, startPosition, parentContext))
    {
        IsReadOnly = isReadOnly;
        IsStatic = isStatic;
        Parents = parents;
        StaticParentReferences = new Reference[Parents.Length];

        Context newContext = new($"<{ExecutableName} static initialization context>", false, StartPosition, Context, Context);

        AddStaticParents(result);
        if (result.ShouldReturn)
        {
            newContext.Release();
            return;
        }

        interpreter.VisitNode(body, newContext, null, IsStatic ? AccessMod.Static : AccessMod.None);

        if (IsStatic || result.ShouldReturn)
        {
            newContext.Release();
            return;
        }

        Context.GetStatus status = newContext.Get(null, EzrClassInstance.InitializationFunction, out Reference functionReference, AccessMod.LocalScope);
        if ((status != Context.GetStatus.Ok && status != Context.GetStatus.UndefinedSymbolAccessNotAllowed)
            || (!functionReference.IsEmpty && functionReference.Object is not EzrFunction))
        {
            result.Failure(new EzrUndefinedValueError("Cannot have any object defined in class with reserved name \"initialize\", unless it is a function!", _executionContext, StartPosition, EndPosition));
            newContext.Release();

            return;
        }
        else if (functionReference.IsEmpty)
        {
            newContext.Release();
            return;
        }

        if ((functionReference.AccessibilityModifiers & AccessMod.Constant) != AccessMod.Constant || (functionReference.AccessibilityModifiers & AccessMod.Private) == AccessMod.Private)
        {
            result.Failure(new EzrIllegalOperationError($"Initialization function for class \"{ExecutableName}\" must be a non-private constant!", _executionContext, StartPosition, EndPosition));
            newContext.Release();

            return;
        }

        EzrFunction initializationFunction = (EzrFunction)functionReference.Object;
        Parameters = initializationFunction.Parameters;
        KeywordArguments = initializationFunction.KeywordArguments;

        newContext.Release();
    }

    /// <summary>
    /// Creates a new class from the innards of an existing one.
    /// </summary>
    /// <param name="name">The name of the executable.</param>
    /// <param name="body">The source code body of the executable.</param>
    /// <param name="parameters">The source code of the class's parameters and their default values.</param>
    /// <param name="keywordArguments">The position in source code and name of the variable for the class's extra keyword arguments.</param>
    /// <param name="parents">The parents of the class.</param>
    /// <param name="staticParentReferences">The references to the class's static parents.</param>
    /// <param name="isReadOnly">Is this a read-only class?</param>
    /// <param name="isStatic">Is this a static class?</param>
    /// <param name="staticContext">The internal static context.</param>
    /// <param name="parentContext">The parent context.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    /// 
    /// \bug The name given to the parents is problematic, as two parents could have the same executable name.
    public EzrClass(string? name, Node body, (string Name, Node Node)[] parameters, (Position StartPosition, Position EndPosition, string Name)? keywordArguments, EzrClass[] parents, Reference[] staticParentReferences, bool isReadOnly, bool isStatic, Context staticContext, Context parentContext, Position startPosition, Position endPosition) : base(name, body, parameters, keywordArguments, parentContext, startPosition, endPosition, staticContext)
    {
        Parents = parents;
        IsReadOnly = isReadOnly;
        IsStatic = isStatic;
        StaticParentReferences = staticParentReferences;
    }

    private void AddStaticParents(RuntimeResult result)
    {
        Context.SetNewLinkedContexts(Parents.Length);

        int anonymousParents = 0;
        for (int i = 0; i < Parents.Length; i++)
        {
            EzrClass parent = Parents[i];
            if (IsStatic && !parent.IsStatic)
            {
                result.Failure(new EzrIllegalOperationError("A static class can only inherit from other static classes!", _executionContext, StartPosition, EndPosition));
                return;
            }

            parent.Update(Context, StartPosition, EndPosition);
            Reference reference = ReferencePool.Get(parent, AccessMod.PrivateStaticConstant);

            if (Array.Exists(StaticParentReferences, existingParent => existingParent?.Object?.HashTag == parent.HashTag))
            {
                result.Failure(new EzrIllegalOperationError($"Cannot inherit from the same class \"{parent.ExecutableName}\" more than once!", _executionContext, StartPosition, EndPosition));
                return;
            }

            string name;
            if (parent.IsAnonymous)
            {
                name = i == 0 ? "parent" : $"parent_anonymous_{anonymousParents}";
                anonymousParents++;
            }
            else
                name = i == 0 ? "parent" : $"parent_{parent.ExecutableName}";

            reference.UpdateName(name);
            Context.Set(null, name, reference);

            StaticParentReferences[i] = reference;
            Context.LinkedContexts[i] = reference.Object.Context;
        }
    }

    /// \bug The name given to the parents is problematic, as two parents could have the same executable name.
    private Reference[] AddParents(Reference[] arguments, Context context, Interpreter interpreter, RuntimeResult result)
    {
        List<Reference> parentReferences = [];
        int anonymousParents = 0;

        for (int i = 0; i < Parents.Length; i++)
        {
            EzrClass parent = Parents[i];
            if (parent.IsStatic)
                continue;

            parent.Execute(arguments, interpreter, result, true);
            if (result.ShouldReturn)
                return [];

            Reference reference = ReferencePool.Get(result.Reference.Object, AccessMod.PrivateConstant);
            reference.Object.Update(context, StartPosition, EndPosition);

            if (parentReferences.Exists(existingParent => (existingParent.Object as EzrClassInstance)?.Class?.HashTag == parent.HashTag))
            {
                result.Failure(new EzrIllegalOperationError($"Cannot inherit from the same class \"{parent.ExecutableName}\" more than once!", _executionContext, StartPosition, EndPosition));
                return [];
            }

            string name;
            if (parent.IsAnonymous)
            {
                name = i == 0 ? "parent" : $"parent_anonymous_{anonymousParents}";
                anonymousParents++;
            }
            else
                name = i == 0 ? "parent" : $"parent_{parent.ExecutableName}";

            reference.UpdateName(name);
            context.Set(null, name, reference);

            parentReferences.Add(reference);
            context.LinkedContexts[i] = reference.Object.Context;
        }

        return [.. parentReferences];
    }

    /// <summary>
    /// Creates a new instance of the class.
    /// </summary>
    /// <param name="arguments">The arguments of the execution.</param>
    /// <param name="interpreter">The interpreter to be used in execution.</param>
    /// <param name="result">Runtime result for carrying any errors.</param>
    /// <param name="ignoreExtraArguments">Should the arguments checker ignore extra parameters?</param>
    private void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result, bool ignoreExtraArguments)
    {
        if (IsStatic)
        {
            result.Failure(new EzrIllegalOperationError("Cannot create instance of a static class!", Context, StartPosition, EndPosition));
            return;
        }

        Context newContext = new($"<{TypeName} \"{ExecutableName}\" instance>", false, StartPosition, Context, Context, Parents.Length);
        Reference[] instanceParentReferences = AddParents(arguments, newContext, interpreter, result);
        if (result.ShouldReturn)
            return;

        IEzrObject instance = new EzrClassInstance(this, instanceParentReferences, arguments, ignoreExtraArguments, newContext, Body, IsReadOnly, interpreter, result, _executionContext, StartPosition, EndPosition);
        if (result.ShouldReturn)
            return;

        result.Success(ReferencePool.Get(instance, AccessMod.PrivateConstant));
    }

    /// <summary>
    /// Creates a new instance of the class.
    /// </summary>
    /// <inheritdoc/>
    public override void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        Execute(arguments, interpreter, result, false);
    }

    /// <inheritdoc/>
    public IMutable<IEzrMutableObject>? DeepCopy(RuntimeResult result)
    {
        Context? copy = Context.DeepCopy(result, []);
        if (result.ShouldReturn)
            return null;

        Reference[] staticParentReferences = new Reference[StaticParentReferences.Length];
        for (int i = 0; i < StaticParentReferences.Length; i++)
        {
            string name = StaticParentReferences[i].Name;
            Context.GetStatus status = copy!.Get(null, name, out Reference reference);

            if (status != Context.GetStatus.Ok)
            {
                result.Failure(new EzrUndefinedValueError($"Could not access static parent \"{name}\" for copy! (Something's really gone wrong)", Context, StartPosition, EndPosition));
                return null;
            }

            staticParentReferences[i] = reference;
            copy.LinkedContexts[i] = reference.Object.Context;
        }

        return new EzrClass(IsAnonymous ? null : ExecutableName, Body, Parameters, KeywordArguments, Parents, staticParentReferences, IsReadOnly, IsStatic, copy!, _creationContext, StartPosition, EndPosition);
    }
}
