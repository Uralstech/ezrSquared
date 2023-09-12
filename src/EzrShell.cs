using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace EzrSquared.EzrShell
{
    using EzrCommon;
    using EzrErrors;
    using EzrLexer;
    using EzrParser;

    /// <summary>
    /// The built-in shell for ezrSquared.
    /// </summary>
    internal class Shell
    {
        private const string Version = "0.1.0";

        private const string ConsoleGraphicsInfo = $"""
                                      | ezr² v{Version}
                                      | Online documentation:
                                      | https://uralstech.github.io/ezrSquared/Introduction.html
                                      | Feature requests and bug reports:
                                      | https://github.com/Uralstech/ezrSquared/issues
                                      | GitHub repository:
            __________________________| https://github.com/Uralstech/ezrSquared/
            """;

        private const string ConsoleGraphicsLetterE = """
            

              ___
             / _ \
            |  __/
             \___|
            """;
        private const string ConsoleGraphicsLetterZ = """

            
                    ____
                   |_  /
                    / /
                   /___|
            """;
        private const string ConsoleGraphicsLetterR = """


                          _ __ 
                         | '__|
                         | |
                         |_|
            """;
        private const string ConsoleGraphicsSquaredSymbol = """
                                 ___
                                |_  )
                                 / /
                                /___|
            
            
            """;

        private static void PrintSmallConsoleGraphics()
        {
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(0, 0);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write('e');

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write('z');

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write('r');

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write('²');

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" v{Version}");
        }

        private static void PrintBigConsoleGraphics()
        {
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.Black;

            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(ConsoleGraphicsInfo);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.SetCursorPosition(0, 0);
            Console.Write(ConsoleGraphicsSquaredSymbol);

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.SetCursorPosition(0, 0);
            Console.Write(ConsoleGraphicsLetterR);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(0, 0);
            Console.Write(ConsoleGraphicsLetterZ);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(0, 0);
            Console.Write(ConsoleGraphicsLetterE);
            Console.Write("\n\n");
        }

        public static void Main(string[] args)
        {
            string filePath;
            string[] commandLineArguments = Environment.GetCommandLineArgs();
            if (commandLineArguments.Length == 1)
                filePath = string.Empty;
            else if (commandLineArguments.Length > 1 && File.Exists(commandLineArguments[1]))
            {
                filePath = commandLineArguments[1];
                //PrintLexerOutput(filePath, File.ReadAllText(filePath));

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }
            else
            {
                ConsoleColor previousForegroundColor = Console.ForegroundColor;
                ConsoleColor previousBackgroundColor = Console.BackgroundColor;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine("Intended use (square brackets indicate optional arguments): ezrSquared [file.ezr2] [file.eout]");

                Console.ForegroundColor = previousForegroundColor;
                Console.BackgroundColor = previousBackgroundColor;
                return;
            }

            Console.OutputEncoding = Encoding.Unicode;

#if PLATFORM_WINDOWS
            if (Console.WindowWidth > 84)
                PrintBigConsoleGraphics();
            else
                PrintSmallConsoleGraphics();
#else
            PrintConsoleGraphic();
#endif

            while (true)
            {
                string? input = GetInput();

                if (!string.IsNullOrEmpty(input))
                {
                    PrintLexerOutput("shell", input, out List<Token> tokens, out Error? error);
                    if (error != null)
                    {
                        PrintError(error.ToString());
                        continue;
                    }

                    ParseResult result = new Parser(tokens).Parse();
                    if (result.Error != null)
                    {
                        PrintError(result.Error.ToString());
                        continue;
                    }

                    PrintResult(result.Node.ToString());
                }
            }
        }

        private static void PrintError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
        }

        private static void PrintResult(string result)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(result);
        }

        private static string? GetInput()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(">>> ");

            return Console.ReadLine();
        }

        private static void PrintLexerOutput(string file, string script, out List<Token> tokens, out Error? error)
        {
            if (new Lexer(file, script).Tokenize(out tokens, out error))
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                for (int i = 0; i < tokens.Count; i++)
                    Console.WriteLine($"( '{tokens[i].Value}', {Enum.GetName(typeof(TokenType), tokens[i].Type)}, {Enum.GetName(typeof(TokenTypeGroup), tokens[i].TypeGroup)} )");
            }
        }
    }
}