namespace EzrSquared.Syntax.Errors;

/// <summary>
/// The <see cref="SyntaxError"/> returned when multiple <see cref="SyntaxError"/> objects need to be returned to the user.
/// </summary>
/// <param name="parent">The 'parent' error, or the error that occurred first.</param>
/// <param name="child">The 'child' error, or the error that occurred because of the <paramref name="parent"/>.</param>
internal class StackedSyntaxError(SyntaxError parent, SyntaxError child) : SyntaxError("Multiple errors", $"{parent}\n\nDue to the above error, another one occurred:\n{child}", parent._startPosition, parent._endPosition)
{
    /// <summary>
    /// Creates the formatted text representation of the <see cref="StackedSyntaxError"/>, which shows all child <see cref="SyntaxError"/> objects as the 'details'.
    /// </summary>
    /// <returns>The formatted text.</returns>
    public override string ToString()
    {
        return _details;
    }
}
