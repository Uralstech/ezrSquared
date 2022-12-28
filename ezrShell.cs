using ezrSquared.Main;
using ezrSquared.Errors;
using ezrSquared.Values;
using ezrSquared.General;
using static ezrSquared.Constants.constants;
using System.Text.RegularExpressions;

namespace ezrSquared.Shell
{
    public class ezrShell
    {
        public static void Main(string[] args)
        {
            string filepath = string.Empty;
            if (args.Length > 0)
                filepath = args[0].Replace("\\", "\\\\");

            ezr instance = new ezr();
            Console.WriteLine($"ezr² biShell version- ({VERSION}) release- [{VERSION_DATE}]");
            Regex regex = new Regex("\\s");

            string[] commands = new string[] { "switch mode", "run code", "quit shell" };
            bool isScript = false;
            string script = string.Empty;
            int scriptLine = 1;


            context runtimeContext = new context("<main>", ezr.globalPredefinedContext, new position(0, 0, 0, "<main>", ""), false);
            runtimeContext.symbolTable = new symbolTable(ezr.globalPredefinedSymbolTable);

            while (true)
            {
                if (string.IsNullOrEmpty(filepath))
                {
                    if (isScript)
                        Console.Write($"({scriptLine}) > ");
                    else
                        Console.Write($"> ");

                    string? input = Console.ReadLine();
                    if (input == null || regex.Replace(input, "") == "") continue;

                    if (commands.Contains(input))
                    {
                        if (input == "switch mode")
                        {
                            isScript = !isScript;
                            scriptLine = 1;
                            script = "";
                            continue;
                        }
                        else if (input == "run code")
                            isScript = false;
                        else if (input == "quit shell")
                            break;
                    }
                    else
                        script += input;

                    if (isScript)
                    {
                        script += '\n';
                        scriptLine++;
                        continue;
                    }
                }
                else
                {
                    script = $"run(\"{filepath}\")";
                    filepath = string.Empty;
                }

                error? error = instance.run("<ezr² biShell>", script, runtimeContext, out item? result);
                if (error != null) Console.WriteLine(error.asString());
                else if (result != null)
                {
                    item[] results = ((array)result).storedValue;
                    if (results.Length == 1) Console.WriteLine(results[0].ToString());
                    else Console.WriteLine(result.ToString());
                }

                script = "";
            }
        }
    }
}