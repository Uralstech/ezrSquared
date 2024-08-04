namespace EzrSquared;

/// <summary>
/// The representation of a position in the script.
/// </summary>
/// <param name="index">The index of the <see cref="Position"/> in the script.</param>
/// <param name="line">The line number of the <see cref="Position"/> in the script.</param>
/// <param name="file">The file name/path of the script.</param>
/// <param name="script">The script as text.</param>
public class Position(int index, int line, string file, string script)
{
    /// <summary>
    /// A position that does not exist.
    /// </summary>
    public readonly static Position None = new(int.MinValue, int.MinValue, string.Empty, string.Empty);

    /// <summary>
    /// The index of the <see cref="Position"/> in the script.
    /// </summary>
    public int Index = index;

    /// <summary>
    /// The line number of the <see cref="Position"/> in the script.
    /// </summary>
    public int Line = line;

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
    public void Advance(char currentChar)
    {
        Index++;

        if (currentChar == '\n')
            Line++;
    }

    /// <summary>
    /// Reverses to the given index.
    /// </summary>
    /// <param name="index">The index to reverse to.</param>
    /// <param name="lineDecrement">The decrement for <see cref="Line"/>.</param>
    public void ReverseTo(int index, int lineDecrement)
    {
        Index = index;
        Line -= lineDecrement;
    }

    /// <summary>
    /// Advances the <see cref="Position"/> and increments <see cref="Index"/> by 1.
    /// </summary>
    public void Advance()
    {
        Index++;
    }

    /// <summary>
    /// Creates a copy of the <see cref="Position"/> object.
    /// </summary>
    /// <returns>The copy.</returns>
    public Position Copy()
    {
        return new Position(Index, Line, File, Script);
    }
}
