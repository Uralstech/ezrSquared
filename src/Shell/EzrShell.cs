using EzrSquared;
using EzrSquared.Executor;
using EzrSquared.Runtime.Types.Collections;
using System;
using System.IO;
using System.Text;

namespace EzrSquaredCli;

/// <summary>
/// The built-in shell for ezrSquared.
/// </summary>
internal class Shell
{
    private const string Version = "0.9.0";

    private static bool s_showLexerOutput;
    private static bool s_showParserOutput;
    private static string s_filePath = string.Empty;

    public static void Main()
    {
        Console.OutputEncoding = Encoding.Unicode;

        string[] commandLineArguments = Environment.GetCommandLineArgs();
        if (!ParseCommandLineArguments(commandLineArguments))
            return;

#if DEBUG
        Console.Write("File to read (press the 'enter' key to enter interactive mode): ");
        string? filePath = Console.ReadLine();

        if (File.Exists(filePath))
            s_filePath = filePath;
        else if (!string.IsNullOrEmpty(filePath))
        {
            EzrShellConsoleHelper.ShowError($"File not found: \"{filePath}\"");
            Console.Write("Press any key to enter interactive mode...");
            Console.ReadKey();
        }
#endif

        if (!string.IsNullOrEmpty(s_filePath))
            ExecuteFile();
        else
            InteractiveMode();

        Console.ResetColor();
    }

    private static bool ParseCommandLineArguments(string[] arguments)
    {
        for (int i = 1; i < arguments.Length; i++)
        {
            switch (arguments[i])
            {
                case "-l":
                case "--lexer-output":
                    s_showLexerOutput = true;
                    break;
                case "-p":
                case "--parser-output":
                    s_showParserOutput = true;
                    break;
                case "-h":
                case "--help":
                    ShowHelp();
                    Console.ResetColor();
                    return false;
                default:
                    if (string.IsNullOrEmpty(s_filePath) && File.Exists(arguments[i]))
                        s_filePath = arguments[i];
                    else
                    {
                        EzrShellConsoleHelper.ShowError("Invalid argument or file not found.");
                        return false;
                    }
                    break;
            }
        }

        return true;
    }

    private static void ShowHelp()
    {
        EzrShellConsoleHelper.ShowOutput("""
            Help for the `ezrSquared` command:
                Intended use:
                    ezrSquared [file] [-h or --help] [-l or --lexer-output] [-p or --parser-output]

                file                    : File/script to execute. If not given, starts in interactive mode.
                -h or --help            : Show this help screen.
                -l or --lexer-output    : Show the output of the Lexer. Only for debugging purposes.
                -p or --parser-output   : Show the output of the Parser. Only for debugging purposes.
            """);
    }

    private static void ExecuteFile()
    {
        CodeExecutor.CreateRuntimeContext(s_filePath);
        CodeExecutor.PopulateRuntimeContext();

        ExecuteCode(File.ReadAllText(s_filePath, Encoding.UTF8));
        EzrShellConsoleHelper.WaitForUser();
    }

    private static void InteractiveMode()
    {
        if (Console.WindowWidth > 110)
            EzrShellConsoleHelper.PrintBigConsoleGraphics(Version);
        else
            EzrShellConsoleHelper.PrintSmallConsoleGraphics(Version);

        CodeExecutor.CreateRuntimeContext("shell");
        CodeExecutor.PopulateRuntimeContext();

        while (true)
        {
            string? input = EzrShellConsoleHelper.GetShellInput();
            if (input?.Trim()?.Equals("\\editor", StringComparison.OrdinalIgnoreCase) == true)
                input = EzrShellEditor.StartShellEditor();

            if (!string.IsNullOrEmpty(input))
                ExecuteCode(input);
        }
    }

    private static bool ExecuteCode(string script)
    {
        ExecutionResult result = CodeExecutor.Execute(script);
        if (s_showLexerOutput)
            EzrShellConsoleHelper.ShowVerbose($"Lexer output:\n - {string.Join<Token>("\n - ", result.Tokens)}");

        if (s_showParserOutput)
            EzrShellConsoleHelper.ShowVerbose($"Parser output:\n{result.Ast}");

        if (!result.Success)
        {
            EzrShellConsoleHelper.ShowError(result.GetErrorMessage());
            return false;
        }

        string outputText = result.Result is EzrArray array && array.Value.Length == 1
            ? array.Value[0].ToString(CodeExecutor.Interpreter.RuntimeResult)
            : result.Result!.ToString(CodeExecutor.Interpreter.RuntimeResult);

        if (CodeExecutor.Interpreter.RuntimeResult.Error is not null)
            EzrShellConsoleHelper.ShowError(CodeExecutor.Interpreter.RuntimeResult.Error.ToPureString(CodeExecutor.Interpreter.RuntimeResult));
        else
            EzrShellConsoleHelper.ShowOutput(outputText);
        return true;
    }
}