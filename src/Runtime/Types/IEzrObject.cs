using EzrSquared.Runtime.Nodes;
using EzrSquared.Runtime.Types.Core.Text;

namespace EzrSquared.Runtime.Types;

/// <summary>
/// An object in the ezr² language.
/// </summary>
public interface IEzrObject
{
    /// <summary>
    /// The name of the type of this object, in plain text, all lowercase. Spaces <i>are</i> allowed.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// The tag of the type of this object, similar to C# namespace naming conventions.
    /// </summary>
    public string Tag { get; }

    /// <summary>
    /// The hash of <see cref="Tag"/>.
    /// </summary>
    public int HashTag { get; }

    /// <summary>
    /// The starting position of the object in source code.
    /// </summary>
    public Position StartPosition { get; }

    /// <summary>
    /// The ending position of the object in source code.
    /// </summary>
    public Position EndPosition { get; }

    /// <summary>
    /// The <see cref="Runtime.Context"/> of the object.
    /// </summary>
    public Context Context { get; }

    /// <summary>
    /// Updates the context and position of the object.
    /// </summary>
    /// <param name="context">The new context of the object.</param>
    /// <param name="startPosition">The new starting position of the object.</param>
    /// <param name="endPosition">The new ending position of the object.</param>
    public void Update(Context context, Position startPosition, Position endPosition);

    /// <summary>
    /// Compares the object to another, checks equality.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void ComparisonEqual(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Compares the object to another, checks unequality.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void ComparisonNotEqual(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Compares the object to another, checks if the current object is less than the other.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void ComparisonLessThan(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Compares the object to another, checks if the current object is greater than the other.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void ComparisonGreaterThan(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Compares the object to another, checks if the current object is less than or equal to the other.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void ComparisonLessThanOrEqual(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Compares the object to another, checks if the current object is greater than or equal to the other.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void ComparisonGreaterThanOrEqual(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Performs the addition operation between the current object and another.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void Addition(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Performs the subtraction operation between the current object and another.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void Subtraction(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Performs the multiplication operation between the current object and another.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void Multiplication(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Performs the division operation between the current object and another.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void Division(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Performs the modulo operation between the current object and another.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void Modulo(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Performs the power or exponent operation between the current object and another.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void Power(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Negates the current object.
    /// </summary>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void Negation(RuntimeResult result);

    /// <summary>
    /// Affirms the current object.
    /// </summary>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void Affirmation(RuntimeResult result);

    /// <summary>
    /// Performs the bit-wise OR operation between the current object and another.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void BitwiseOr(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Performs the bit-wise X-OR operation between the current object and another.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void BitwiseXOr(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Performs the bit-wise AND operation between the current object and another.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void BitwiseAnd(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Performs the bit-wise left-shift operation between the current object and another.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void BitwiseLeftShift(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Performs the bit-wise right-shift operation between the current object and another.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void BitwiseRightShift(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Bit-wise negates the current object.
    /// </summary>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void BitwiseNegation(RuntimeResult result);

    /// <summary>
    /// Checks if the other object is contained in the current object.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void HasValueContained(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Checks if the other object is NOT contained in the current object.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void NotHasValueContained(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Inverts the current object, like, for example, true to false.
    /// </summary>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public void Inversion(RuntimeResult result);

    /// <summary>
    /// Evaluates the current object as a boolean value.
    /// </summary>
    /// <param name="result">Runtime result for carrying any errors.</param>
    /// <returns>The evaluated value.</returns>
    public bool EvaluateBoolean(RuntimeResult result);

    /// <summary>
    /// Interprets an AST node in context of the current object.
    /// </summary>
    /// <param name="code">The node to interpret.</param>
    /// <param name="callingContext">The context calling on this action.</param>
    /// <param name="interpreter">The interpreter to use.</param>
    /// <param name="result">Runtime result for carrying any errors.</param>
    /// <param name="ignoreUndefinedVariable">Should the interpretation ignore undefined variables? Useful in getting empty variable references.</param>
    public void Interpret(Node code, Context callingContext, Interpreter interpreter, RuntimeResult result, bool ignoreUndefinedVariable = false);

    /// <summary>
    /// Executes the current object, like a function.
    /// </summary>
    /// <param name="arguments">The arguments of the execution.</param>
    /// <param name="interpreter">The interpreter to be used in execution.</param>
    /// <param name="result">Runtime result for carrying any errors.</param>
    public void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result);

    /// <summary>
    /// Strictly compares the current object to another, taking into account inheritance.
    /// </summary>
    /// <param name="other">The other object in the operation.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    public bool StrictEquals(IEzrObject other, RuntimeResult result);

    /// <summary>
    /// Evaluates the current object as its hash.
    /// </summary>
    /// <param name="result">Runtime result for carrying any errors.</param>
    /// <returns>The evaluated value.</returns>
    public int ComputeHashCode(RuntimeResult result);

    /// <summary>
    /// Evaluates the current object as a string value.
    /// </summary>
    /// <param name="result">Runtime result for carrying any errors.</param>
    /// <returns>The evaluated value.</returns>
    public string ToString(RuntimeResult result);

    /// <summary>
    /// Evaluates the current object as a string value.
    /// </summary>
    /// <remarks>
    /// This is used to show the 'real' string representation of the object. Like, for example, <see cref="ToString(RuntimeResult)"/> will<br/>
    /// return "example" when called on an <see cref="EzrString"/> object with value "example", but this function will return example (without quotes).
    /// </remarks>
    /// <param name="result">Runtime result for carrying any errors.</param>
    /// <returns>The evaluated value.</returns>
    public string ToPureString(RuntimeResult result);
}
