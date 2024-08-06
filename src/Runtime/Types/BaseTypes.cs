using EzrSquared.Runtime.Collections;
using EzrSquared.Runtime.Nodes;
using EzrSquared.Runtime.Types.Collections;
using EzrSquared.Runtime.Types.Core;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Core.Numerics;
using EzrSquared.Runtime.Types.Core.Text;
using System.Numerics;

namespace EzrSquared.Runtime.Types;

/// <summary>
/// An invalid, sort of "empty" object, to use instead of <see langword="null"/>.
/// </summary>
internal class EzrRuntimeInvalidObject : EzrObject
{
    /// <summary>
    /// The static instance of the invalid object.
    /// </summary>
    internal static EzrRuntimeInvalidObject s_instance = new();

    /// <summary>
    /// Creates a new <see cref="EzrRuntimeInvalidObject"/>.
    /// </summary>
    EzrRuntimeInvalidObject() : base(Context.Empty, Context.Empty, Position.None, Position.None) { }
}

/// <summary>
/// The base root class of all built-in objects. Provides utility functions and bare-minimum operator handling.
/// </summary>
public abstract class EzrObject : IEzrObject
{
    /// <inheritdoc/>
    public virtual string TypeName { get; protected internal set; } = string.Empty;

    /// <inheritdoc/>
    public virtual string Tag { get; protected internal set; } = string.Empty;

    /// <summary>
    /// The hash of <see cref="Tag"/>.
    /// </summary>
    private int _hashTag = int.MinValue;

    /// <inheritdoc/>
    public int HashTag
    {
        get
        {
            if (_hashTag == int.MinValue)
                _hashTag = Tag.GetHashCode();
            return _hashTag;
        }
    }

    /// <inheritdoc/>
    public Position StartPosition { get; private set; }

    /// <inheritdoc/>
    public Position EndPosition { get; private set; }

    /// <inheritdoc/>
    public Context Context { get; private set; }

    /// <summary>
    /// Is the current object read-only?
    /// </summary>
    public bool IsReadOnly { get; protected internal set; } = true;

    /// <summary>
    /// The current context in which the operation is being executed.
    /// </summary>
    protected internal Context _executionContext;

    /// <summary>
    /// The context under which the object was created.
    /// </summary>
    protected internal Context _creationContext;

    /// <summary>
    /// Creates a new object with the specified parent context and position.
    /// </summary>
    /// <param name="creationContext">The context in which this object was created in.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrObject(Context creationContext, Position startPosition, Position endPosition)
    {
        StartPosition = startPosition;
        EndPosition = endPosition;

        _executionContext = _creationContext = creationContext;
        Context = new Context(TypeName, false, startPosition, _creationContext, _creationContext.StaticContext);
    }

    /// <summary>
    /// Creates a new object with the specified internal context, parent context and position.
    /// </summary>
    /// <param name="context">The internal context, if <see langword="null"/>, creates a new one.</param>
    /// <param name="creationContext">The context in which this object was created in.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrObject(Context? context, Context creationContext, Position startPosition, Position endPosition)
    {
        StartPosition = startPosition;
        EndPosition = endPosition;

        _executionContext = _creationContext = creationContext;
        Context = context ?? new Context(TypeName, false, startPosition, _creationContext, _creationContext.StaticContext);
    }

    /// <summary>
    /// Changes <see cref="_creationContext"/> and the parent of <see cref="Context"/>. Be careful when you use this function.
    /// </summary>
    /// <param name="newCreationContext">The new creation context.</param>
    public void UpdateCreationContext(Context newCreationContext)
    {
        _creationContext = newCreationContext;
    }

    /// <inheritdoc/>
    public void Update(Context context, Position startPosition, Position endPosition)
    {
        StartPosition = startPosition;
        EndPosition = endPosition;
        _executionContext = context;

        Context.UpdatePosition(StartPosition);
    }

    /// <inheritdoc/>
    public virtual void ComparisonEqual(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public virtual void ComparisonNotEqual(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public virtual void ComparisonLessThan(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public virtual void ComparisonGreaterThan(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public virtual void ComparisonLessThanOrEqual(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public virtual void ComparisonGreaterThanOrEqual(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public virtual void Addition(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public virtual void Subtraction(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public virtual void Multiplication(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public virtual void Division(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public virtual void Modulo(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public virtual void Power(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public virtual void Negation(RuntimeResult result)
    {
        result.Failure(IllegalOperation());
    }

    /// <inheritdoc/>
    public virtual void Affirmation(RuntimeResult result)
    {
        result.Failure(IllegalOperation());
    }

    /// <inheritdoc/>
    public virtual void BitwiseOr(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public virtual void BitwiseXOr(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public virtual void BitwiseAnd(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public virtual void BitwiseLeftShift(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public virtual void BitwiseRightShift(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public virtual void BitwiseNegation(RuntimeResult result)
    {
        result.Failure(IllegalOperation());
    }

    /// <inheritdoc/>
    public virtual void HasValueContained(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation(other, false));
    }

    /// <inheritdoc/>
    public virtual void NotHasValueContained(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation(other, false));
    }

    /// <inheritdoc/>
    public virtual void Inversion(RuntimeResult result)
    {
        result.Failure(IllegalOperation());
    }

    /// <inheritdoc/>
    public virtual bool EvaluateBoolean(RuntimeResult result)
    {
        result.Failure(IllegalOperation());
        return false;
    }

    /// <inheritdoc/>
    public virtual void Interpret(Node code, Context callingContext, Interpreter interpreter, RuntimeResult result, bool ignoreUndefinedVariable = false)
    {
        interpreter.VisitNode(code, Context, callingContext, AccessMod.LocalScope, ignoreUndefinedVariable);
        if (result.ShouldReturn)
            return;

        Reference reference = result.Reference;
        if (reference.IsEmpty && IsReadOnly)
        {
            result.Failure(new EzrUndefinedValueError($"You cannot create nor change members of a read-only object!", Context, StartPosition, EndPosition));
            return;
        }
        
        if (IsReadOnly)
            result.Success(
                ReferencePool.Get(
                    reference.Object,
                    AccessMod.PrivateConstant,
                    reference.Name
                )
            );
    }

    /// <inheritdoc/>
    public virtual void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        result.Failure(IllegalOperation());
    }

    /// <inheritdoc/>
    public virtual bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        result.Failure(IllegalOperation());
        return false;
    }

    /// <inheritdoc/>
    public virtual int ComputeHashCode(RuntimeResult result)
    {
        result.Failure(IllegalOperation());
        return int.MinValue;
    }

    /// <inheritdoc/>
    public virtual string ToString(RuntimeResult result)
    {
        return TypeName;
    }

    /// <inheritdoc/>
    public virtual string ToPureString(RuntimeResult result)
    {
        return ToString(result);
    }

    /// <summary>Destructor.</summary>
    ~EzrObject()
    {
        Context.Release();
    }

    /// <summary>
    /// Creates a new "nothing" constant.
    /// </summary>
    /// <returns>The constant.</returns>
    protected internal Reference NewNothingConstant()
    {
        return ReferencePool.Get(EzrConstants.Nothing, AccessMod.PrivateConstant);
    }

    /// <summary>
    /// Creates a new boolean constant.
    /// </summary>
    /// <param name="value">The raw boolean value.</param>
    /// <returns>The constant.</returns>
    protected internal Reference NewBooleanConstant(bool value)
    {
        return ReferencePool.Get(value
            ? EzrConstants.True
            : EzrConstants.False, AccessMod.PrivateConstant);
    }

    /// <summary>
    /// Creates a new integer constant.
    /// </summary>
    /// <param name="value">The raw integer value.</param>
    /// <returns>The constant.</returns>
    protected internal Reference NewIntegerConstant(BigInteger value)
    {
        return ReferencePool.Get(new EzrInteger(value, _executionContext, StartPosition, EndPosition), AccessMod.PrivateConstant);
    }

    /// <summary>
    /// Creates a new float constant.
    /// </summary>
    /// <param name="value">The raw float value.</param>
    /// <returns>The constant.</returns>
    protected internal Reference NewFloatConstant(double value)
    {
        return ReferencePool.Get(new EzrFloat(value, _executionContext, StartPosition, EndPosition), AccessMod.PrivateConstant);
    }

    /// <summary>
    /// Creates a new string constant.
    /// </summary>
    /// <param name="value">The raw string value.</param>
    /// <returns>The constant.</returns>
    protected internal Reference NewStringConstant(string value)
    {
        return ReferencePool.Get(new EzrString(value, _executionContext, StartPosition, EndPosition), AccessMod.PrivateConstant);
    }

    /// <summary>
    /// Creates a new character list constant.
    /// </summary>
    /// <param name="value">The raw float value.</param>
    /// <returns>The constant.</returns>
    protected internal Reference NewCharacterListConstant(string value)
    {
        return ReferencePool.Get(new EzrCharacterList(value, _executionContext, StartPosition, EndPosition), AccessMod.PrivateConstant);
    }

    /// <summary>
    /// Creates a new character constant.
    /// </summary>
    /// <param name="value">The raw character value.</param>
    /// <returns>The constant.</returns>
    protected internal Reference NewCharacterConstant(char value)
    {
        return ReferencePool.Get(new EzrCharacter(value, _executionContext, StartPosition, EndPosition), AccessMod.PrivateConstant);
    }

    /// <summary>
    /// Creates a new array constant.
    /// </summary>
    /// <param name="elements">The raw array value.</param>
    /// <returns>The constant.</returns>
    protected internal Reference NewArrayConstant(IEzrObject[] elements)
    {
        return ReferencePool.Get(new EzrArray(elements, _executionContext, StartPosition, EndPosition), AccessMod.PrivateConstant);
    }

    /// <summary>
    /// Creates a new list constant.
    /// </summary>
    /// <param name="elements">The raw list value.</param>
    /// <returns>The constant.</returns>
    protected internal Reference NewListConstant(RuntimeEzrObjectList elements)
    {
        return ReferencePool.Get(new EzrList(elements, _executionContext, StartPosition, EndPosition), AccessMod.PrivateConstant);
    }

    /// <summary>
    /// Creates a new dictionary constant.
    /// </summary>
    /// <param name="dictionary">The raw dictionary value.</param>
    /// <returns>The constant.</returns>
    protected internal Reference NewDictionaryConstant(RuntimeEzrObjectDictionary dictionary)
    {
        return ReferencePool.Get(new EzrDictionary(dictionary, _executionContext, StartPosition, EndPosition), AccessMod.PrivateConstant);
    }

    /// <summary>
    /// Creates an illegal operation error based on the current context and position.
    /// </summary>
    /// <returns>The error.</returns>
    protected internal EzrIllegalOperationError IllegalOperation()
    {
        return new($"Illegal operation for type \"{TypeName}\"!", _executionContext, StartPosition, EndPosition);
    }

    /// <summary>
    /// Creates an illegal operation error based on the current context and position and another object.
    /// </summary>
    /// <returns>The error.</returns>
    protected internal EzrIllegalOperationError IllegalOperation(IEzrObject other, bool isRightHandSide = true)
    {
        return 
            isRightHandSide
            ? new EzrIllegalOperationError($"Illegal operation for types \"{TypeName}\" and \"{other.TypeName}\"!", _executionContext, StartPosition, other.EndPosition)
            : new EzrIllegalOperationError($"Illegal operation for types \"{other.TypeName}\" and \"{TypeName}\"!", _executionContext, other.StartPosition, EndPosition);
    }
}
