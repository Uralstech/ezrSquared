using System;
using System.Text;

namespace EzrSquared;

/// <summary>
/// Error interface for all errors.
/// </summary>
public interface IEzrError
{
    /// <summary>
    /// The name of the <see cref="IEzrError"/>.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// The reason why the <see cref="IEzrError"/> occurred.
    /// </summary>
    public string Details { get; }

    /// <summary>
    /// The starting <see cref="Position"/> of the <see cref="IEzrError"/>.
    /// </summary>
    public Position ErrorStartPosition { get; }

    /// <summary>
    /// The ending <see cref="Position"/> of the <see cref="IEzrError"/>.
    /// </summary>
    public Position ErrorEndPosition { get; }

    /// <summary>
    /// Creates formatted text which contains the text between <paramref name="startPosition"/> and <paramref name="endPosition"/>, underlined with tilde (~) symbols.
    /// </summary>
    /// <param name="startPosition">The starting position of the underlining.</param>
    /// <param name="endPosition">The ending position of the underlining.</param>
    /// <returns>The formatted text and the actual starting line number of the error.</returns>
    internal protected static (int AdjustedLineNumber, string SourceWithUnderline) SourceWithUnderline(Position startPosition, Position endPosition)
    {
        string text = startPosition.Script;
        int textLength = text.Length;

        int startIndex = startPosition.Index;
        int line = startPosition.Line;

        // Adjust the start index if it points to a newline character and the range is empty
        if (startIndex < textLength && text[startIndex] == '\n' && endPosition.Index - startIndex == 1)
        {
            char currentChar = text[startIndex];
            while (startIndex > 0 && char.IsWhiteSpace(currentChar))
            {
                if (currentChar == '\n')
                    line--;

                startIndex--;
                currentChar = text[startIndex];
            }
        }

        // Find the start of the line containing the start position
        // I always forget that LastIndexOf counts backwards from startIndex.
        int lineStart = text.LastIndexOf('\n', Math.Min(startIndex, textLength - 1)) + 1;

        // Find the end of the line containing the end position
        int lineEnd = endPosition.Index < textLength ? text.IndexOf('\n', endPosition.Index) : -1;
        if (lineEnd == -1)
            lineEnd = textLength - (startPosition.Index - startIndex);

        // Move start to the first non-whitespace character on the line
        while (lineStart < textLength && char.IsWhiteSpace(text[lineStart]))
            lineStart++;

        // Determine if the error spans multiple lines
        int firstNewLineAfterStart = text.IndexOf('\n', lineStart);
        bool isMultiline = firstNewLineAfterStart >= 0 && firstNewLineAfterStart < lineEnd;

        // Build the result string
        StringBuilder result = new("  ");
        result.Append(text, lineStart, lineEnd - lineStart);

        if (isMultiline)
            result.Replace("\n", "\n  ");
        else
        {
            result.Append('\n');
            result.Append(' ', Math.Max(0, Math.Min(startIndex + 1, startPosition.Index) - lineStart) + 2);

            result.Append('~', Math.Max(1, endPosition.Index - startPosition.Index));
        }

        return (line, result.ToString());
    }
}
