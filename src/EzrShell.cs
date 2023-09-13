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

        private struct Settings
        {
            public bool IsInteractive;
            public bool ShowLexerOutput;
            public bool ShowParserOutput;
            public string File;

            public Settings()
            {
                IsInteractive = false;
                ShowLexerOutput = false;
                ShowParserOutput = false;
                File = string.Empty;
            }
        }

        private static ConsoleColor s_previousForegroundColor;
        private static ConsoleColor s_previousBackgroundColor;

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

        private static void ShowError(string message)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
        }

        private static void ShowOutput(string message)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
        }

        private static void ShowVerbose(string message)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(message);
        }

        private static string? GetShellInput()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.Write(">>> ");
            return Console.ReadLine();
        }

        private static bool ParseCommandLineArguments(string[] arguments, out Settings settings)
        {
            settings = new Settings();
            bool isFirstArgument = true;

            for (int i = 1; i < arguments.Length; i++)
            {
                if (arguments[i] == "-l" || string.Compare(arguments[1], "--lexer-output", true) == 0)
                    settings.ShowLexerOutput = true;
                else if (arguments[i] == "-p" || string.Compare(arguments[1], "--parser-output", true) == 0)
                    settings.ShowLexerOutput = true;
                else if (File.Exists(arguments[i]))
                    settings.File = arguments[i];
                else if (isFirstArgument && (arguments[i] == "-h" || string.Compare(arguments[1], "--help", true) == 0))
                {
                    ShowOutput("""
                        Help for the `ezrSquared` command:
                            Intended use:
                                ezrSquared [file] [-h or --help] [-l or --lexer-output] [-p or --parser-output]

                            file                    : File/script to execute. If not given, starts in interative mode.
                            -h or --help            : Show this help screen.
                            -l or --lexer-output    : Show the output of the Lexer. Only for debugging purposes.
                            -p or --parser-output   : Show the output of the Parser. Only for debugging purposes.
                        """);
                    return false;
                }
                else
                {
                    ShowError("Intended use:\n\tezrSquared [file] [-l | --lexer-output] [-p | --parser-output]");
                    return false;
                }
            
                isFirstArgument = false;
            }

            return true;
        }

        public static void Main()
        {
            s_previousBackgroundColor = Console.BackgroundColor;
            s_previousForegroundColor = Console.ForegroundColor;
            Console.OutputEncoding = Encoding.Unicode;

            string[] commandLineArguments = Environment.GetCommandLineArgs();
            if (!ParseCommandLineArguments(commandLineArguments, out Settings settings))
                return;

#if DEBUG
            settings.ShowLexerOutput = true;
            settings.ShowParserOutput = true;

            Console.Write("File to read (press the 'enter' key to enter interactive mode): ");
            string? filePath = Console.ReadLine();

            if (string.IsNullOrEmpty(filePath))
                settings.IsInteractive = true;
            else if (File.Exists(filePath))
                settings.File = filePath;
            else
            {
                ShowError($"File not found: \"{filePath}\"");

                Console.Write("Press any key to continue...");
                Console.ReadKey();

                settings = new Settings();
            }
#endif

            if (!string.IsNullOrEmpty(settings.File))
                ExecuteFile(settings);
            else if (settings.IsInteractive)
                InteractiveMode(settings);

            Console.BackgroundColor = s_previousBackgroundColor;
            Console.ForegroundColor = s_previousForegroundColor;
        }

        private static void ExecuteFile(Settings settings)
        {
            Lexer lexer = new Lexer(Path.GetFileNameWithoutExtension(settings.File), File.ReadAllText(settings.File, Encoding.UTF8).ReplaceLineEndings("\n"));
            Error? error = lexer.Tokenize(out List<Token> tokens);
            if (settings.ShowLexerOutput)
            {
                ShowVerbose("Lexer output:");
                for (int i = 0; i < tokens.Count; i++)
                    ShowVerbose($" - {tokens[i]}");
            }

            if (error != null)
            {
                ShowError(error.ToString());
                return;
            }

            Parser parser = new Parser(tokens);
            ParseResult result = parser.Parse();
            if (result.Error != null)
            {
                ShowError(result.Error.ToString());
                return;
            }

            if (settings.ShowParserOutput)
                ShowVerbose($"Parser output:\n{result.Node}");

            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }

        private static void InteractiveMode(Settings settings)
        {
#if PLATFORM_WINDOWS
            if (Console.WindowWidth > 84)
                PrintBigConsoleGraphics();
            else
                PrintSmallConsoleGraphics();
#else
                PrintSmallConsoleGraphics();
#endif

            while (true)
            {
                string? input = GetShellInput();

                if (!string.IsNullOrEmpty(input))
                {
                    Lexer lexer = new Lexer("shell", input);
                    Error? error = lexer.Tokenize(out List<Token> tokens);
                    if (settings.ShowLexerOutput)
                    {
                        ShowVerbose("Lexer output:");
                        for (int i = 0; i < tokens.Count; i++)
                            ShowVerbose($" - {tokens[i]}");
                    }

                    if (error != null)
                    {
                        ShowError(error.ToString());
                        continue;
                    }

                    Parser parser = new Parser(tokens);
                    ParseResult result = parser.Parse();
                    if (result.Error != null)
                    {
                        ShowError(result.Error.ToString());
                        continue;
                    }

                    if (settings.ShowParserOutput)
                        ShowVerbose($"Parser output:\n{result.Node}");
                }
            }
        }
    }
}