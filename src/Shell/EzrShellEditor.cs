using System;
using System.IO;
using System.Text;

namespace EzrSquaredCli;

internal class EzrShellEditor
{
    internal static string? StartShellEditor()
    {
        StringBuilder scriptBuilder = new();
        int line = 1;
        string filepath = string.Empty;

        bool running = true;
        while (running)
        {
            string? input = EzrShellConsoleHelper.GetShellInput(line: line);
            if (input is null)
                continue;

            switch (input.Trim())
            {
                case "\\run":
                    return scriptBuilder.ToString();
                case "\\clear":
                    ConfirmAndClearScript(scriptBuilder);
                    break;
                case "\\save":
                    SaveScriptToFile(scriptBuilder, ref filepath);
                    break;
                case "\\quit":
                    running = !ConfirmAndQuitEditor();
                    break;

                default:
                    scriptBuilder.AppendLine(input);
                    line++;

                    AppendScriptToFile(input, filepath);
                    break;
            }
        }

        return null;
    }

    private static void ConfirmAndClearScript(StringBuilder scriptBuilder)
    {
        string? confirmation = EzrShellConsoleHelper.GetShellInput("Are you sure? You will lose this script forever. (y/N) ");
        if (confirmation?.Equals("y", StringComparison.OrdinalIgnoreCase) == true)
            scriptBuilder.Clear();
    }

    private static bool ConfirmAndQuitEditor()
    {
        string? confirmation = EzrShellConsoleHelper.GetShellInput("Are you sure? You will lose this script forever. (y/N) ");
        return confirmation?.Equals("y", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static void SaveScriptToFile(StringBuilder scriptBuilder, ref string filepath)
    {
        string? userFilePath = EzrShellConsoleHelper.GetShellInput("Save filepath: ");
        if (string.IsNullOrEmpty(userFilePath) || !IsValidPath(userFilePath))
        {
            EzrShellConsoleHelper.ShowError("Invalid file path!");
            return;
        }

        userFilePath = Path.GetFullPath(userFilePath);
        if (File.Exists(userFilePath))
        {
            string? confirmation = EzrShellConsoleHelper.GetShellInput($"There is already a file at \"{userFilePath}\". If you continue with the operation, ezr² will erase the file. Do you want to continue? (y/N) ");
            if (confirmation?.Equals("y", StringComparison.OrdinalIgnoreCase) != true)
                return;
        }

        string? directory = Path.GetDirectoryName(userFilePath);
        if (string.IsNullOrEmpty(directory))
        {
            EzrShellConsoleHelper.ShowError("Could not get directory path!");
            return;
        }

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        filepath = userFilePath;
        File.WriteAllText(filepath, scriptBuilder.ToString());
    }

    private static void AppendScriptToFile(string input, string filepath)
    {
        if (!string.IsNullOrEmpty(filepath))
            File.AppendAllText(filepath, $"\n{input}");
    }

    private static bool IsValidPath(string path, bool allowRelativePaths = false)
    {
        try
        {
            string fullPath = Path.GetFullPath(path);
            return allowRelativePaths || Path.IsPathRooted(fullPath);
        }
        catch
        {
            return false;
        }
    }
}
