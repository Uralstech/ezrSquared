using EzrSquared.Runtime.Nodes;
using EzrSquared.Runtime.Types.Core;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Core.Numerics;
using EzrSquared.Runtime.Types.Core.Text;
using System;

namespace EzrSquared.Runtime.Types.Executables;

/// <summary>
/// The "instance of a class" type object?
/// </summary>
public class EzrClassInstance : EzrObject, IEzrMutableObject
{
    /// <summary>Special function name for initializer (constructor).</summary>
    public const string InitializationFunction = "initialize";

    /// <summary>Special function name for 'this = other' operation.</summary>
    public const string IsEqualFunction = "is_equal";

    /// <summary>Special function name for 'this ! other' operation.</summary>
    public const string IsNotEqualFunction = "is_inequal";

    /// <summary>Special function name for 'this &lt; other' operation.</summary>
    public const string IsLessThanFunction = "is_less_than";

    /// <summary>Special function name for 'this &gt; other' operation.</summary>
    public const string IsGreaterThanFunction = "is_greater_than";

    /// <summary>Special function name for 'this &lt;= other' operation.</summary>
    public const string IsLessThanOrEqualFunction = "is_less_than_or_equal";

    /// <summary>Special function name for 'this &gt;= other' operation.</summary>
    public const string IsGreaterThanOrEqualFunction = "is_greater_than_or_equal";

    /// <summary>Special function name for 'this + other' operation.</summary>
    public const string AdditionFunction = "addition";

    /// <summary>Special function name for 'this - other' operation.</summary>
    public const string SubtractionFunction = "subtraction";

    /// <summary>Special function name for 'this * other' operation.</summary>
    public const string MultiplicationFunction = "multiplication";

    /// <summary>Special function name for 'this / other' operation.</summary>
    public const string DivisionFunction = "division";

    /// <summary>Special function name for 'this % other' operation.</summary>
    public const string ModuloFunction = "modulo";

    /// <summary>Special function name for 'this ^ other' operation.</summary>
    public const string PowerFunction = "power";

    /// <summary>Special function name for '-this' operation.</summary>
    public const string NegationFunction = "negation";

    /// <summary>Special function name for '+this' operation.</summary>
    public const string AffirmationFunction = "affirmation";

    /// <summary>Special function name for 'this | other' operation.</summary>
    public const string BitwiseOrFunction = "bitwise_or";

    /// <summary>Special function name for 'this \ other' operation.</summary>
    public const string BitwiseXOrFunction = "bitwise_xor";

    /// <summary>Special function name for 'this &amp; other' operation.</summary>
    public const string BitwiseAndFunction = "bitwise_and";

    /// <summary>Special function name for 'this &lt;&lt; other' operation.</summary>
    public const string BitwiseLeftShiftunction = "bitwise_left_shift";

    /// <summary>Special function name for 'this &gt;&gt; other' operation.</summary>
    public const string BitwiseRightShiftFunction = "bitwise_right_shift";

    /// <summary>Special function name for '~this' operation.</summary>
    public const string BitwiseNegationFunction = "bitwise_negation";

    /// <summary>Special function name for 'other in this' operation.</summary>
    public const string ContainsFunction = "contains";

    /// <summary>Special function name for 'other not in this' operation.</summary>
    public const string DoesNotContainFunction = "does_not_contain";

    /// <summary>Special function name for 'invert this' operation.</summary>
    public const string InversionFunction = "inversion";

    /// <summary>Special function name for evaluation of the 'this' into a boolean.</summary>
    public const string EvaluateBooleanFunction = "evaluate_boolean";

    /// <summary>Special function name for 'this(arguments)' operation.</summary>
    public const string CalledFunction = "call_received";

    /// <summary>Special function name for strict equality checking of 'this' and 'other'.</summary>
    public const string StrictComparisonFunction = "strict_equals";

    /// <summary>Special function name for hashing 'this'.</summary>
    public const string GetHashCodeFunction = "get_hash_code";

    /// <summary>Special function name for the string representation of 'this'.</summary>
    public const string ToStringFunction = "to_string";

    /// <summary>Special function name for the "pure" string representation of 'this'.</summary>
    public const string ToPureStringFunction = "to_real_string";

    /// <summary>
    /// The interpreter for executing parts of the class instance.
    /// </summary>
    public readonly Interpreter Interpreter;

    /// <summary>
    /// The parent class of the class instance.
    /// </summary>
    public readonly EzrClass Class;

    /// <summary>
    /// The references to the class instance's parents.
    /// </summary>
    public readonly Reference[] ParentReferences;

    /// <summary>
    /// Creates a new instance of the class <paramref name="class"/>.
    /// </summary>
    /// <param name="class">The parent class of the current object.</param>
    /// <param name="parentReferences">The references to the class instance's parents.</param>
    /// <param name="arguments">The arguments for the creation of the object.</param>
    /// <param name="ignoreExtraArguments">Should the arguments checker ignore extra arguments?</param>
    /// <param name="context">The internal context of the object.</param>
    /// <param name="body">The source code body of the object.</param>
    /// <param name="readOnly">Is the object read-only?</param>
    /// <param name="interpreter">The interpreter for executing parts of the object.</param>
    /// <param name="result">Runtime result for carrying errors.</param>
    /// <param name="parentContext">The parent context.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrClassInstance(EzrClass @class, Reference[] parentReferences, Reference[] arguments, bool ignoreExtraArguments, Context context, Node body, bool readOnly, Interpreter interpreter, RuntimeResult result, Context parentContext, Position startPosition, Position endPosition) : base(context, parentContext, startPosition, endPosition)
    {
        Class = @class;
        TypeName = Class.ExecutableName;
        Tag = $"{Class.Tag}.Instance";

        Interpreter = interpreter;
        IsReadOnly = readOnly;

        ParentReferences = parentReferences;
        Context.Set(null, "this", ReferencePool.Get(this, AccessMod.PrivateConstant));

        Context tempStaticContext = new($"<{TypeName} instance initialization context>", true, StartPosition, Context.StaticContext);
        Context? trueStaticContext = Context.StaticContext;

        Context.UpdateStaticContext(tempStaticContext);
        Interpreter.VisitNode(body, Context, null, AccessMod.None);
        Context.UpdateStaticContext(trueStaticContext);

        tempStaticContext.Release();
        if (result.ShouldReturn)
            return;

        bool hasInitializationFunction = Context.Get(null, InitializationFunction, out Reference functionReference, AccessMod.LocalScope, true) == Context.GetStatus.Ok
                                            && functionReference.Object is EzrFunction function;

        if (hasInitializationFunction)
            (functionReference.Object as EzrFunction)!.Execute(arguments, interpreter, result, ignoreExtraArguments);
        else if (arguments.Length > 0 && !ignoreExtraArguments)
            result.Failure(new EzrUnexpectedArgumentError("Did not expected any arguments!", context, StartPosition, EndPosition));
    }

    /// <summary>
    /// Creates a new instance of the class <paramref name="class"/>.
    /// </summary>
    /// <param name="class">The parent class of the current object.</param>
    /// <param name="parentReferences">The references to the class instance's parents.</param>
    /// <param name="context">The internal context of the object.</param>
    /// <param name="readOnly">Is the object read-only?</param>
    /// <param name="interpreter">The interpreter for executing parts of the object.</param>
    /// <param name="parentContext">The parent context.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrClassInstance(EzrClass @class, Reference[] parentReferences, bool readOnly, Interpreter interpreter, Context context, Context parentContext, Position startPosition, Position endPosition) : base(context, parentContext, startPosition, endPosition)
    {
        Class = @class;
        TypeName = Class.ExecutableName;
        Tag = $"{Class.Tag}.Instance";

        Interpreter = interpreter;
        IsReadOnly = readOnly;

        ParentReferences = parentReferences;
        Context.Set(null, "this", ReferencePool.Get(this, AccessMod.PrivateConstant));
    }

    #region Operator overrides
    /// <summary>
    /// Gets a function based on the name and number of parameters.
    /// </summary>
    /// <param name="name">The name of the function.</param>
    /// <param name="parameters">The number of parameters required for the function.</param>
    /// <param name="functionReference">The resulting reference to the function.</param>
    /// <returns><see langword="true"/> if successful, <see langword="false"/> otherwise.</returns>
    private bool GetFunction(string name, int parameters, out Reference functionReference)
    {
        return Context.Get(null, name, out functionReference, AccessMod.LocalScope) == Context.GetStatus.Ok
            && functionReference.Object is EzrFunction function
            && (parameters == -1 || function.Parameters.Length == parameters);
    }

    /// <inheritdoc/>
    public override void ComparisonEqual(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(IsEqualFunction, 1, out Reference objectReference))
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
        else
            result.Success(NewBooleanConstant(false));
    }

    /// <inheritdoc/>
    public override void ComparisonNotEqual(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(IsNotEqualFunction, 1, out Reference objectReference))
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
        else
            result.Success(NewBooleanConstant(true));
    }

    /// <inheritdoc/>
    public override void ComparisonLessThan(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(IsLessThanFunction, 1, out Reference objectReference))
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
        else
            result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public override void ComparisonGreaterThan(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(IsGreaterThanFunction, 1, out Reference objectReference))
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
        else
            result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public override void ComparisonLessThanOrEqual(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(IsLessThanOrEqualFunction, 1, out Reference objectReference))
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
        else
            result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public override void ComparisonGreaterThanOrEqual(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(IsGreaterThanOrEqualFunction, 1, out Reference objectReference))
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
        else
            result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public override void Addition(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(AdditionFunction, 1, out Reference objectReference))
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
        else
            result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public override void Subtraction(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(SubtractionFunction, 1, out Reference objectReference))
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
        else
            result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public override void Multiplication(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(MultiplicationFunction, 1, out Reference objectReference))
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
        else
            result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public override void Division(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(DivisionFunction, 1, out Reference objectReference))
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
        else
            result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public override void Modulo(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(ModuloFunction, 1, out Reference objectReference))
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
        else
            result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public override void Power(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(PowerFunction, 1, out Reference objectReference))
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
        else
            result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public override void Negation(RuntimeResult result)
    {
        if (GetFunction(NegationFunction, 0, out Reference objectReference))
            objectReference.Object.Execute([], Interpreter, result);
        else
            result.Failure(IllegalOperation());
    }

    /// <inheritdoc/>
    public override void Affirmation(RuntimeResult result)
    {
        if (GetFunction(AffirmationFunction, 0, out Reference objectReference))
            objectReference.Object.Execute([], Interpreter, result);
        else
            result.Failure(IllegalOperation());
    }

    /// <inheritdoc/>
    public override void BitwiseOr(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(BitwiseOrFunction, 1, out Reference objectReference))
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
        else
            result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public override void BitwiseXOr(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(BitwiseXOrFunction, 1, out Reference objectReference))
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
        else
            result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public override void BitwiseAnd(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(BitwiseAndFunction, 1, out Reference objectReference))
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
        else
            result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public override void BitwiseLeftShift(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(BitwiseLeftShiftunction, 1, out Reference objectReference))
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
        else
            result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public override void BitwiseRightShift(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(BitwiseRightShiftFunction, 1, out Reference objectReference))
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
        else
            result.Failure(IllegalOperation(other));
    }

    /// <inheritdoc/>
    public override void BitwiseNegation(RuntimeResult result)
    {
        if (GetFunction(BitwiseNegationFunction, 0, out Reference objectReference))
            objectReference.Object.Execute([], Interpreter, result);
        else
            result.Failure(IllegalOperation());
    }

    /// <inheritdoc/>
    public override void HasValueContained(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(ContainsFunction, 1, out Reference objectReference))
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
        else
            result.Failure(IllegalOperation(other, false));
    }

    /// <inheritdoc/>
    public override void NotHasValueContained(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(DoesNotContainFunction, 1, out Reference objectReference))
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
        else
            result.Failure(IllegalOperation(other, false));
    }

    /// <inheritdoc/>
    public override void Inversion(RuntimeResult result)
    {
        if (GetFunction(InversionFunction, 0, out Reference objectReference))
            objectReference.Object.Execute([], Interpreter, result);
        else
            result.Failure(IllegalOperation());
    }

    /// <inheritdoc/>
    public override bool EvaluateBoolean(RuntimeResult result)
    {
        if (GetFunction(EvaluateBooleanFunction, 0, out Reference objectReference))
        {
            objectReference.Object.Execute([], Interpreter, result);
            if (result.ShouldReturn)
                return false;

            IEzrObject ezrObject = result.Reference.Object;
            if (ezrObject is not EzrBoolean evaluation)
            {
                result.Failure(new EzrUnexpectedTypeError($"Expected output of function \"{EvaluateBooleanFunction}\" to be of type boolean but got object of type \"{ezrObject.TypeName}\"!", Context, ezrObject.StartPosition, ezrObject.EndPosition));
                return false;
            }

            return evaluation.Value;
        }

        result.Failure(IllegalOperation());
        return false;
    }

    /// <inheritdoc/>
    public override void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        if (GetFunction(CalledFunction, -1, out Reference objectReference))
            objectReference.Object.Execute(arguments, Interpreter, result);
        else
            result.Failure(IllegalOperation());
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        if (GetFunction(StrictComparisonFunction, 1, out Reference objectReference))
        {
            objectReference.Object.Execute([ReferencePool.Get(other, AccessMod.Private)], Interpreter, result);
            if (result.ShouldReturn)
                return false;

            IEzrObject ezrObject = result.Reference.Object;
            if (ezrObject is not EzrBoolean evaluation)
            {
                result.Failure(new EzrUnexpectedTypeError($"Expected output of function \"{StrictComparisonFunction}\" to be of type boolean but got object of type \"{ezrObject.TypeName}\"!", Context, ezrObject.StartPosition, ezrObject.EndPosition));
                return false;
            }

            return evaluation.Value;
        }

        result.Failure(IllegalOperation());
        return false;
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        if (GetFunction(GetHashCodeFunction, 0, out Reference objectReference))
        {
            objectReference.Object.Execute([], Interpreter, result);
            if (result.ShouldReturn)
                return 0;

            IEzrObject ezrObject = result.Reference.Object;
            if (ezrObject is not EzrInteger evaluation)
            {
                result.Failure(new EzrUnexpectedTypeError($"Expected output of function \"{GetHashCodeFunction}\" to be of type integer but got object of type \"{ezrObject.TypeName}\"!", Context, ezrObject.StartPosition, ezrObject.EndPosition));
                return 0;
            }

            if (evaluation.TryGetIntRepresentation(out int hashCode))
                return HashCode.Combine(HashTag, hashCode);

            result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", Context, evaluation.StartPosition, evaluation.EndPosition));
            return 0;
        }

        result.Failure(IllegalOperation());
        return 0;
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        if (GetFunction(ToStringFunction, 0, out Reference objectReference))
        {
            objectReference.Object.Execute([], Interpreter, result);
            if (result.ShouldReturn)
                return string.Empty;

            IEzrObject ezrObject = result.Reference.Object;
            if (ezrObject is not EzrString and not EzrCharacterList and not EzrCharacter)
            {
                result.Failure(new EzrUnexpectedTypeError($"Expected output of function \"{ToStringFunction}\" to be of types string, character list or character but got object of type \"{ezrObject.TypeName}\"!", Context, ezrObject.StartPosition, ezrObject.EndPosition));
                return string.Empty;
            }

            return ezrObject switch
            {
                EzrString stringValue => stringValue.Value,
                EzrCharacterList characterListValue => characterListValue.StringValue,
                EzrCharacter characterValue => characterValue.Value.ToString(),
                _ => throw new ArgumentException($"Unexpected type {ezrObject.GetType().Name} for string conversion!")
            };
        }

        return $"<object of type \"{TypeName}\">";
    }

    /// <inheritdoc/>
    public override string ToPureString(RuntimeResult result)
    {
        if (GetFunction(ToPureStringFunction, 0, out Reference objectReference))
        {
            objectReference.Object.Execute([], Interpreter, result);
            if (result.ShouldReturn)
                return string.Empty;

            IEzrObject ezrObject = result.Reference.Object;
            if (ezrObject is not EzrString and not EzrCharacterList and not EzrCharacter)
            {
                result.Failure(new EzrUnexpectedTypeError($"Expected output of function \"{ToPureStringFunction}\" to be of types string, character list or character but got object of type \"{ezrObject.TypeName}\"!", Context, ezrObject.StartPosition, ezrObject.EndPosition));
                return string.Empty;
            }

            return ezrObject switch
            {
                EzrString stringValue => stringValue.Value,
                EzrCharacterList characterListValue => characterListValue.StringValue,
                EzrCharacter characterValue => characterValue.Value.ToString(),
                _ => throw new ArgumentException($"Unexpected type {ezrObject.GetType().Name} for string conversion!")
            };
        }

        return ToString(result);
    }
    #endregion

    /// <inheritdoc/>
    public IMutable<IEzrMutableObject>? DeepCopy(RuntimeResult result)
    {
        Context? copy = Context.DeepCopy(result, [this]);
        if (result.ShouldReturn)
            return null;

        Reference[] parentReferences = new Reference[ParentReferences.Length];
        for (int i = 0; i < ParentReferences.Length; i++)
        {
            string name = ParentReferences[i].Name;
            Context.GetStatus status = copy!.Get(null, name, out Reference reference);

            if (status != Context.GetStatus.Ok)
            {
                result.Failure(new EzrUndefinedValueError($"Could not access parent \"{name}\" for copy! (Something's really gone wrong)", Context, StartPosition, EndPosition));
                return null;
            }

            parentReferences[i] = reference;
            copy.LinkedContexts[i] = reference.Object.Context;
        }

        return new EzrClassInstance(Class, parentReferences, IsReadOnly, Interpreter, copy!, CreationContext, StartPosition, EndPosition);
    }
}
