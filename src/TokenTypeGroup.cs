namespace EzrSquared;

/// <summary>
/// The type group of a <see cref="Token"/>.
/// </summary>
/// <remarks>
/// If you want to see which group a token type is in, check the documentation for that type.
/// </remarks>
public enum TokenTypeGroup : ushort
{
    /// <summary>
    /// Groups primitive-type tokens, like integers or strings.
    /// </summary>
    Value,

    /// <summary>
    /// Groups keyword tokens.
    /// </summary>
    Keyword,

    /// <summary>
    /// Groups QuickSyntax keyword tokens.
    /// </summary>
    Qeyword,

    /// <summary>
    /// Groups variable assignment symbol tokens.
    /// </summary>
    AssignmentSymbol,

    /// <summary>
    /// Groups general symbol tokens.
    /// </summary>
    Symbol,

    /// <summary>
    /// Groups special tokens.
    /// </summary>
    Special
}
