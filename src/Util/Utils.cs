using System;
using System.Text;

namespace EzrSquared.Util;

/// <summary>
/// Utilities functions used in ezrSquared.
/// </summary>
public static class Utils
{
    /// <summary>
    /// <see cref="long"/> value for the last generated unique identifier.
    /// </summary>
    private static long s_currentId = 0;

    /// <summary>
    /// Generates an incremental unique identifier.
    /// </summary>
    /// <returns>A new unique identifier.</returns>
    public static long GetNextUniqueId()
    {
        return System.Threading.Interlocked.Increment(ref s_currentId);
    }

    /// <summary>
    /// Converts an index to a power of two, so that the index can be used like an enum flag.
    /// </summary>
    /// <remarks>
    /// This function is used to check if all arguments have been provided to <see cref="Runtime.Types.CSharpWrappers.SourceWrappers.EzrSharpSourceExecutableWrapper"/> and <see cref="Runtime.Types.Executables.EzrRuntimeExecutable"/> types.<br/>
    /// The indices of the given arguments are converted to powers of two and bitwise-ored together, then bitwise-anded with the index of a defined parameter which is also converted to a power of two.<br/>
    /// Finally, if the result is the same as the power of two of the parameter's index, this tells the interpreter that the particular required parameter has been provided.
    /// </remarks>
    /// <param name="index">The index to be converted.</param>
    /// <returns>The power of two.</returns>
    public static int IndexToFlag(int index)
    {
        return (index++ < 3) ? index : (4 * index) - 8;
    }

    /// <summary>
    /// Creates formatted text which contains the text between <paramref name="startPosition"/> and <paramref name="endPosition"/>, underlined with tilde (~) symbols.
    /// </summary>
    /// <returns>The formatted text.</returns>
    public static (int AdjustedLineNumber, string SourceWithUnderline) SourceWithUnderline(Position startPosition, Position endPosition)
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

    /// <summary>
    /// Converts a string from PascalCase to snake_case.
    /// </summary>
    /// <param name="text">The text to convert in PascalCase.</param>
    /// <returns>The converted text in snake_case.</returns>
    public static string PascalToSnakeCase(string text)
    {
        StringBuilder result = new();
        result.Append(char.ToLowerInvariant(text[0]));

        for (int i = 1; i < text.Length; ++i)
        {
            char c = text[i];
            if (char.IsUpper(c))
                result.Append('_').Append(char.ToLowerInvariant(c));
            else
                result.Append(c);
        }

        return result.ToString();
    }
}
