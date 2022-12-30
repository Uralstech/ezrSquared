namespace ezrSquared.Constants
{
    public static class constants
    {
        public const string VERSION = "prerelease-1.0.0.0.1";
        public const string VERSION_DATE = "30.12.2022";

        public const string LETTERS_UNDERSCORE = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
        public const string ALPHANUM_UNDERSCORE = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
        public const string DIGITS = "0123456789";
        public const string DIGITS_PERIOD = "0123456789.";

        public static readonly string[] KEYWORDS = { "item", "and", "or", "invert", "if", "else", "do", "count", "from", "as", "to", "step", "while", "function", "with", "end", "return", "skip", "stop", "try", "error", "in", "object", "global", "include" };
        public static readonly string[] QEYWORDS = { "f", "l", "e", "c", "t", "n", "w", "fd", "od", "i", "s", "d", "g", "v" };

        public const string RT_DEFAULT = "any";
        public const string RT_ILLEGALOP = "operation-error";
        public const string RT_UNDEFINED = "undefined-error";
        public const string RT_KEY = "key-error";
        public const string RT_INDEX = "index-error";
        public const string RT_ARGS = "arguments-error";
        public const string RT_TYPE = "type-error";
        public const string RT_MATH = "math-error";
        public const string RT_RUN = "run-error";
        public const string RT_IO = "io-error";
    }
}