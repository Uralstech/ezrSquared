using System;

namespace EzrSquaredCli;

internal static class EzrShellConsoleHelper
{
    private const string ConsoleGraphicsInfo = """
                                                     ▋ ezr² v{0}
                                                     ▋ Online documentation:
                                                     ▋ https://uralstech.github.io/ezrSquared/Introduction.html
                                                     ▋ Feature requests and bug reports:
                                                     ▋ https://github.com/Uralstech/ezrSquared/issues
                                                     ▋ GitHub repository:
                                                     ▋ https://github.com/Uralstech/ezrSquared/
                                                     ▋
                                                     ▋
                                                     ▋
            ▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▋
            """;

    private const string ConsoleGraphicsLetterE = """
        
         ▟███████▙
         ██     ██
         ██     ██
         ████████▛
         ██
         ██
         ██
         ██
         ▜████████
        """;
    private const string ConsoleGraphicsLetterZ = """

                    █████████
                          ▟█▛
                         ▟█▛
                        ▟█▛
                       ▟█▛
                      ▟█▛
                     ▟█▛
                    ▟█▛
                    █████████
        """;
    private const string ConsoleGraphicsLetterR = """

                               ██▟█████▙
                               ██▛    ▜█
                               ██
                               ██
                               ██
                               ██
                               ██
                               ██
                               ██
        """;
    private const string ConsoleGraphicsSquaredSymbol = """
                                          ████▙
                                             ██
                                          ▟███▛
                                          ██
                                          ▜████



        
        """;

    internal static void PrintSmallConsoleGraphics(string version)
    {
        Console.Clear();

        ShowMessage("e", ConsoleColor.Green, true);
        ShowMessage("z", ConsoleColor.Red);
        ShowMessage("r", ConsoleColor.Blue);
        ShowMessage("²", ConsoleColor.Yellow);
        ShowMessage($" v{version}", ConsoleColor.White);
    }

    internal static void PrintBigConsoleGraphics(string version)
    {
        Console.Clear();

        ShowMessage(string.Format(ConsoleGraphicsInfo, version), ConsoleColor.White, true);
        ShowMessage(ConsoleGraphicsSquaredSymbol, ConsoleColor.Yellow, true);
        ShowMessage(ConsoleGraphicsLetterR, ConsoleColor.Blue, true);
        ShowMessage(ConsoleGraphicsLetterZ, ConsoleColor.Red, true);
        ShowMessage($"{ConsoleGraphicsLetterE}\n", ConsoleColor.Green, true);
    }

    internal static void ShowError(string message)
    {
        ShowMessage(message, ConsoleColor.Red);
    }

    internal static void ShowOutput(string message)
    {
        ShowMessage(message, ConsoleColor.White);
    }

    internal static void ShowVerbose(string message)
    {
        ShowMessage(message, ConsoleColor.DarkGray);
    }

    internal static string? GetShellInput(string? prompt = null, int? line = null)
    {
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(line.HasValue ? $"{line} >>> {prompt}" : $">>> {prompt}");
        return Console.ReadLine();
    }

    internal static void WaitForUser()
    {
        Console.ResetColor();

        Console.Write("Press any key to continue...");
        Console.ReadKey();
    }

    private static void ShowMessage(string message, ConsoleColor color, bool resetPosition = false)
    {
        if (resetPosition)
            Console.SetCursorPosition(0, 0);

        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
    }
}
