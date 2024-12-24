namespace EzrSquared;

/// <summary>
/// The representation of a position in the script.
/// </summary>
/// <param name="index">The index of the <see cref="Position"/> in the script.</param>
/// <param name="line">The line number of the <see cref="Position"/> in the script.</param>
/// <param name="file">The file name/path of the script.</param>
/// <param name="script">The script as text.</param>
public readonly struct Position(int index, int line, string file, string script)
{
    /// <summary>
    /// A position that does not exist.
    /// </summary>
    public readonly static Position None = new(0, 0, string.Empty, string.Empty);

    /// <summary>
    /// The index of the <see cref="Position"/> in the script.
    /// </summary>
    public readonly int Index = index;

    /// <summary>
    /// The line number of the <see cref="Position"/> in the script.
    /// </summary>
    public readonly int Line = line;

    /// <summary>
    /// The file name/path of the script.
    /// </summary>
    public readonly string File = file;

    /// <summary>
    /// The script as text.
    /// </summary>
    public readonly string Script = script;

    /// <summary>
    /// Advances the <see cref="Position"/> and increments <see cref="Index"/> by 1. If <paramref name="currentChar"/> is a new-line character, <see cref="Line"/> is also incremented by 1.
    /// </summary>
    /// <param name="currentChar">The character associated with the <see cref="Position"/> before advancing.</param>
    public Position Advance(char currentChar)
    {
        return new Position(Index + 1, currentChar == '\n' ? Line + 1 : Line, File, Script);
    }

    /// <summary>
    /// Advances the <see cref="Position"/> and increments <see cref="Index"/> by 1.
    /// </summary>
    public Position Advance()
    {
        return new Position(Index + 1, Line, File, Script);
    }

    /// <summary>
    /// Reverses to the given index.
    /// </summary>
    /// <param name="index">The index to reverse to.</param>
    /// <param name="lineDecrement">The decrement for <see cref="Line"/>.</param>
    public Position ReverseTo(int index, int lineDecrement)
    {
        return new Position(index, Line - lineDecrement, File, Script);
    }
}
