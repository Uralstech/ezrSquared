using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace EzrSquared.EzrShell
{
    using EzrGeneral;
    using EzrErrors;
    using EzrLexer;

    /// <summary>
    /// The built-in shell for ezrSquared.
    /// </summary>
    internal class Shell
    {
        private const string _ezrVersion = "0.1.0";
        private const string _projectLink = "https://github.com/Uralstech/ezrSquared/";
        private const string _documentationLink = "https://uralstech.github.io/ezrSquared/Introduction.html";
        private const string _issuesLink = "https://github.com/Uralstech/ezrSquared/issues";
        private const string _startUp = "                      ___  | ezr² v{0} Built-In Shell\n                     |_  ) | Online documentation:\n   ___   ____  _ __   / /  | {1}\n  / _ \\ |_  / | '__| /___| | Feature requests and bug reports:\n |  __/  / /  | |          | {2}\n  \\___| /___| |_|          | GitHub repository:\n___________________________| {3}";

        private static void PrintLexerOutput(string file, string script)
        {
            if (new Lexer(file, script).Tokenize(out List<Token> tokens, out Error? error))
            {
                for (int i = 0; i < tokens.Count; i++)
                    Console.WriteLine($"( {tokens[i].Value}, {Enum.GetName(typeof(TokenType), tokens[i].Type)} )");
            }
            else if (error != null)
                Console.WriteLine(error.ToString());
        }

        public static void Main()
        {
            string filePath = string.Empty;

            string[] commandLineArguments = Environment.GetCommandLineArgs();
            if (commandLineArguments.Length > 1 && File.Exists(commandLineArguments[1]))
            {
                filePath = commandLineArguments[1];
                PrintLexerOutput(filePath, File.ReadAllText(filePath));
            }
            else
            {
                Console.OutputEncoding = Encoding.Unicode;
                Console.WriteLine(string.Format(_startUp, _ezrVersion, _documentationLink, _issuesLink, _projectLink));
                while (true)
                {
                    Console.Write(">>> ");
                    string? input = Console.ReadLine();

                    if (!string.IsNullOrEmpty(input))
                        PrintLexerOutput("ezr² Built-In Shell", input);
                }
            }
        }
    }
}