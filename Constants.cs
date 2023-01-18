namespace ezrSquared.Constants
{
    public static class constants
    {
        public const string VERSION = "prerelease-1.1.0.0.1";
        public const string VERSION_DATE = "18.01.2023";

        public const string LETTERS_UNDERSCORE = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
        public const string ALPHANUM_UNDERSCORE = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
        public const string DIGITS = "0123456789";
        public const string DIGITS_PERIOD = "0123456789.";

        public static readonly string[] KEYWORDS = { "item", "and", "or", "invert", "if", "else", "do", "count", "from", "as", "to", "step", "while", "function", "special", "with", "end", "return", "skip", "stop", "try", "error", "in", "object", "global", "include" };
        public static readonly string[] QEYWORDS = { "f", "l", "e", "c", "t", "n", "w", "fd", "sd", "od", "i", "s", "d", "g", "v" };

        public static readonly Dictionary<string, string[]> SPECIALS = new Dictionary<string, string[]>()
        {
            { "compare_equal", new string[1] { "other" } },
            { "compare_not_equal", new string[1] { "other" } },
            { "compare_less_than", new string[1] { "other" } },
            { "compare_greater_than", new string[1] { "other" } },
            { "compare_less_than_or_equal", new string[1] { "other" } },
            { "compare_greater_than_or_equal", new string[1] { "other" } },
            { "compare_and", new string[1] { "other" } },
            { "compare_or", new string[1] { "other" } },

            { "bitwise_or", new string[1] { "other" } },
            { "bitwise_xor", new string[1] { "other" } },
            { "bitwise_and", new string[1] { "other" } },
            { "bitwise_left_shift", new string[1] { "other" } },
            { "bitwise_right_shift", new string[1] { "other" } },
            { "bitwise_not", new string[0] },

            { "addition", new string[1] { "other" } },
            { "subtraction", new string[1] { "other" } },
            { "multiplication", new string[1] { "other" } },
            { "division", new string[1] { "other" } },
            { "modulo", new string[1] { "other" } },
            { "power", new string[1] { "other" } },

            { "invert", new string[0] },
            { "check_in", new string[1] { "other" } },

            { "is_true", new string[0] },
            { "hash", new string[0] },
        };

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