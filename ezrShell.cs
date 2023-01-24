using ezrSquared.Main;
using ezrSquared.Errors;
using ezrSquared.Values;
using ezrSquared.General;
using static ezrSquared.Constants.constants;

namespace ezrSquared.Shell
{
    public class ezrShell
    {
        public static void Main(string[] args)
        {
            string filepath = string.Empty;
            if (args.Length > 0)
                filepath = args[0].Replace("\\", "\\\\");

            Console.WriteLine($"ezr² biShell version- ({VERSION}) release- [{VERSION_DATE}]");

            string[] commands = new string[] { "switch mode", "run code", "quit shell" };
            bool isScript = false;
            string script = string.Empty;
            int scriptLine = 1;

            context runtimeContext = new context("<main>", ezr.globalPredefinedContext, new position(0, 0, 0, "<main>", ""), false);
            runtimeContext.symbolTable = new symbolTable(ezr.globalPredefinedContext.symbolTable);

            while (true)
            {
                if (string.IsNullOrEmpty(filepath))
                {
                    if (isScript)
                        Console.Write($"({scriptLine}) > ");
                    else
                        Console.Write($"> ");

                    string? input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(input)) continue;

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

                if (!string.IsNullOrEmpty(script))
                {
                    error? error = ezr.run("<ezr² biShell>", script, runtimeContext, out item? result);
                    if (error != null) Console.WriteLine(error.asString());
                    else if (result != null)
                    {
                        item[] results = ((array)result).storedValue;
                        if (results.Length == 1) Console.WriteLine(results[0].ToString());
                        else Console.WriteLine(result.ToString());
                    }
                }

                script = "";
            }
        }
    }
}