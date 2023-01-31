using ezrSquared.General;
using System;

namespace ezrSquared.Errors
{
    public abstract class error
    {
        public string name;
        public string details;
        public position startPos;
        public position endPos;

        public error(string name, string details, position startPos, position endPos)
        {
            this.name = name;
            this.details = details;
            this.startPos = startPos;
            this.endPos = endPos;
        }

        public virtual string asString() { return $"(error) {name}: {details} -> File '{startPos.file}', line {startPos.line + 1}\n{stringWithUnderline(startPos.text, startPos, endPos)}"; }

        internal string stringWithUnderline(string text, position startPos, position endPos)
        {
            int start = Math.Max(text[0..((startPos.index <= text.Length) ? startPos.index : text.Length)].LastIndexOf('\n'), 0);
            int end = text.IndexOf('\n', start + 1);
            if (end == -1) end = text.Length;

            string result = text[start..end] + '\n';
            for (int i = 0; i < startPos.column; i++)
                result += ' ';
            for (int i = 0; i < endPos.column - startPos.column; i++)
                result += '~';
            return result.Replace('\t', ' ');
        }
    }

    public class unknownCharacterError : error
    {
        public unknownCharacterError(string details, position startPos, position endPos) : base("Unknown character", details, startPos, endPos) { }
    }

    public class invalidGrammarError : error
    {
        public invalidGrammarError(string details, position startPos, position endPos) : base("Invalid grammar", details, startPos, endPos) { }
    }

    public class overflowError : error
    {
        public overflowError(string details, position startPos, position endPos) : base("Overflow", details, startPos, endPos) { }
    }

    public class runtimeError : error
    {
        public context context;
        public runtimeError(position startPos, position endPos, string tag, string details, context context) : base(tag, details, startPos, endPos) { this.context = context; }

        public override string asString() { return $"{generateTraceback()}(runtime error) : {details} -> tag '{name}'\n\n{stringWithUnderline(startPos.text, startPos, endPos)}"; }

        private string generateTraceback()
        {
            string result = "";
            position? pos = startPos;
            context? context = this.context;

            while (context != null)
            {
                result = $"\t File '{pos.file}', line {pos.line + 1} - In '{context.name}'\n{result}";
                pos = context.parentEntryPos;
                context = context.parent;
            }

            return $"Traceback - most recent call last:\n{result}";
        }
    }
}