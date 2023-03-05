using ezrSquared.Values;
using ezrSquared.Helpers;
using ezrSquared.Errors;
using ezrSquared.General;
using static ezrSquared.Constants.constants;
using System.Collections.Generic;
using System.IO;
using System;

namespace ezrSquared.Libraries.IO
{
    public class console : baseFunction
    {
        private Dictionary<string, ConsoleKey> stringToKeyLookup = new Dictionary<string, ConsoleKey>()
        {
            { "Backspace", ConsoleKey.Backspace },
            { "Tab", ConsoleKey.Tab },
            { "Clear", ConsoleKey.Clear },
            { "Enter", ConsoleKey.Enter },
            { "Pause", ConsoleKey.Pause },
            { "Escape", ConsoleKey.Escape },
            { "Spacebar", ConsoleKey.Spacebar },
            { "PageUp", ConsoleKey.PageUp },
            { "PageDown", ConsoleKey.PageDown },
            { "End", ConsoleKey.End },
            { "Home", ConsoleKey.Home },
            { "LeftArrow", ConsoleKey.LeftArrow },
            { "UpArrow", ConsoleKey.UpArrow },
            { "RightArrow", ConsoleKey.RightArrow },
            { "DownArrow", ConsoleKey.DownArrow },
            { "Select", ConsoleKey.Select },
            { "Print", ConsoleKey.Print },
            { "Execute", ConsoleKey.Execute },
            { "PrintScreen", ConsoleKey.PrintScreen },
            { "Insert", ConsoleKey.Insert },
            { "Delete", ConsoleKey.Delete },
            { "Help", ConsoleKey.Help },
            { "D0", ConsoleKey.D0 },
            { "D1", ConsoleKey.D1 },
            { "D2", ConsoleKey.D2 },
            { "D3", ConsoleKey.D3 },
            { "D4", ConsoleKey.D4 },
            { "D5", ConsoleKey.D5 },
            { "D6", ConsoleKey.D6 },
            { "D7", ConsoleKey.D7 },
            { "D8", ConsoleKey.D8 },
            { "D9", ConsoleKey.D9 },
            { "A", ConsoleKey.A },
            { "B", ConsoleKey.B },
            { "C", ConsoleKey.C },
            { "D", ConsoleKey.D },
            { "E", ConsoleKey.E },
            { "F", ConsoleKey.F },
            { "G", ConsoleKey.G },
            { "H", ConsoleKey.H },
            { "I", ConsoleKey.I },
            { "J", ConsoleKey.J },
            { "K", ConsoleKey.K },
            { "L", ConsoleKey.L },
            { "M", ConsoleKey.M },
            { "N", ConsoleKey.N },
            { "O", ConsoleKey.O },
            { "P", ConsoleKey.P },
            { "Q", ConsoleKey.Q },
            { "R", ConsoleKey.R },
            { "S", ConsoleKey.S },
            { "T", ConsoleKey.T },
            { "U", ConsoleKey.U },
            { "V", ConsoleKey.V },
            { "W", ConsoleKey.W },
            { "X", ConsoleKey.X },
            { "Y", ConsoleKey.Y },
            { "Z", ConsoleKey.Z },
            { "LeftWindows", ConsoleKey.LeftWindows },
            { "RightWindows", ConsoleKey.RightWindows },
            { "Applications", ConsoleKey.Applications },
            { "Sleep", ConsoleKey.Sleep },
            { "NumPad0", ConsoleKey.NumPad0 },
            { "NumPad1", ConsoleKey.NumPad1 },
            { "NumPad2", ConsoleKey.NumPad2 },
            { "NumPad3", ConsoleKey.NumPad3 },
            { "NumPad4", ConsoleKey.NumPad4 },
            { "NumPad5", ConsoleKey.NumPad5 },
            { "NumPad6", ConsoleKey.NumPad6 },
            { "NumPad7", ConsoleKey.NumPad7 },
            { "NumPad8", ConsoleKey.NumPad8 },
            { "NumPad9", ConsoleKey.NumPad9 },
            { "Multiply", ConsoleKey.Multiply },
            { "Add", ConsoleKey.Add },
            { "Separator", ConsoleKey.Separator },
            { "Subtract", ConsoleKey.Subtract },
            { "Decimal", ConsoleKey.Decimal },
            { "Divide", ConsoleKey.Divide },
            { "F1", ConsoleKey.F1 },
            { "F2", ConsoleKey.F2 },
            { "F3", ConsoleKey.F3 },
            { "F4", ConsoleKey.F4 },
            { "F5", ConsoleKey.F5 },
            { "F6", ConsoleKey.F6 },
            { "F7", ConsoleKey.F7 },
            { "F8", ConsoleKey.F8 },
            { "F9", ConsoleKey.F9 },
            { "F10", ConsoleKey.F10 },
            { "F11", ConsoleKey.F11 },
            { "F12", ConsoleKey.F12 },
            { "F13", ConsoleKey.F13 },
            { "F14", ConsoleKey.F14 },
            { "F15", ConsoleKey.F15 },
            { "F16", ConsoleKey.F16 },
            { "F17", ConsoleKey.F17 },
            { "F18", ConsoleKey.F18 },
            { "F19", ConsoleKey.F19 },
            { "F20", ConsoleKey.F20 },
            { "F21", ConsoleKey.F21 },
            { "F22", ConsoleKey.F22 },
            { "F23", ConsoleKey.F23 },
            { "F24", ConsoleKey.F24 },
            { "BrowserBack", ConsoleKey.BrowserBack },
            { "BrowserForward", ConsoleKey.BrowserForward },
            { "BrowserRefresh", ConsoleKey.BrowserRefresh },
            { "BrowserStop", ConsoleKey.BrowserStop },
            { "BrowserSearch", ConsoleKey.BrowserSearch },
            { "BrowserFavorites", ConsoleKey.BrowserFavorites },
            { "BrowserHome", ConsoleKey.BrowserHome },
            { "VolumeMute", ConsoleKey.VolumeMute },
            { "VolumeDown", ConsoleKey.VolumeDown },
            { "VolumeUp", ConsoleKey.VolumeUp },
            { "MediaNext", ConsoleKey.MediaNext },
            { "MediaPrevious", ConsoleKey.MediaPrevious },
            { "MediaStop", ConsoleKey.MediaStop },
            { "MediaPlay", ConsoleKey.MediaPlay },
            { "LaunchMail", ConsoleKey.LaunchMail },
            { "LaunchMediaSelect", ConsoleKey.LaunchMediaSelect },
            { "LaunchApp1", ConsoleKey.LaunchApp1 },
            { "LaunchApp2", ConsoleKey.LaunchApp2 },
            { "Oem1", ConsoleKey.Oem1 },
            { "OemPlus", ConsoleKey.OemPlus },
            { "OemComma", ConsoleKey.OemComma },
            { "OemMinus", ConsoleKey.OemMinus },
            { "OemPeriod", ConsoleKey.OemPeriod },
            { "Oem2", ConsoleKey.Oem2 },
            { "Oem3", ConsoleKey.Oem3 },
            { "Oem4", ConsoleKey.Oem4 },
            { "Oem5", ConsoleKey.Oem5 },
            { "Oem6", ConsoleKey.Oem6 },
            { "Oem7", ConsoleKey.Oem7 },
            { "Oem8", ConsoleKey.Oem8 },
            { "Oem102", ConsoleKey.Oem102 },
            { "Process", ConsoleKey.Process },
            { "Packet", ConsoleKey.Packet },
            { "Attention", ConsoleKey.Attention },
            { "CrSel", ConsoleKey.CrSel },
            { "ExSel", ConsoleKey.ExSel },
            { "EraseEndOfFile", ConsoleKey.EraseEndOfFile },
            { "Play", ConsoleKey.Play },
            { "Zoom", ConsoleKey.Zoom },
            { "NoName", ConsoleKey.NoName },
            { "Pa1", ConsoleKey.Pa1 },
            { "OemClear", ConsoleKey.OemClear }
        };
        private Dictionary<ConsoleKey, string> keyToStringLookup = new Dictionary<ConsoleKey, string>()
        {
            { ConsoleKey.Backspace , "Backspace" },
            { ConsoleKey.Tab , "Tab" },
            { ConsoleKey.Clear , "Clear" },
            { ConsoleKey.Enter , "Enter" },
            { ConsoleKey.Pause , "Pause" },
            { ConsoleKey.Escape , "Escape" },
            { ConsoleKey.Spacebar , "Spacebar" },
            { ConsoleKey.PageUp , "PageUp" },
            { ConsoleKey.PageDown , "PageDown" },
            { ConsoleKey.End , "End" },
            { ConsoleKey.Home , "Home" },
            { ConsoleKey.LeftArrow , "LeftArrow" },
            { ConsoleKey.UpArrow , "UpArrow" },
            { ConsoleKey.RightArrow , "RightArrow" },
            { ConsoleKey.DownArrow , "DownArrow" },
            { ConsoleKey.Select , "Select" },
            { ConsoleKey.Print , "Print" },
            { ConsoleKey.Execute , "Execute" },
            { ConsoleKey.PrintScreen , "PrintScreen" },
            { ConsoleKey.Insert , "Insert" },
            { ConsoleKey.Delete , "Delete" },
            { ConsoleKey.Help , "Help" },
            { ConsoleKey.D0 , "D0" },
            { ConsoleKey.D1 , "D1" },
            { ConsoleKey.D2 , "D2" },
            { ConsoleKey.D3 , "D3" },
            { ConsoleKey.D4 , "D4" },
            { ConsoleKey.D5 , "D5" },
            { ConsoleKey.D6 , "D6" },
            { ConsoleKey.D7 , "D7" },
            { ConsoleKey.D8 , "D8" },
            { ConsoleKey.D9 , "D9" },
            { ConsoleKey.A , "A" },
            { ConsoleKey.B , "B" },
            { ConsoleKey.C , "C" },
            { ConsoleKey.D , "D" },
            { ConsoleKey.E , "E" },
            { ConsoleKey.F , "F" },
            { ConsoleKey.G , "G" },
            { ConsoleKey.H , "H" },
            { ConsoleKey.I , "I" },
            { ConsoleKey.J , "J" },
            { ConsoleKey.K , "K" },
            { ConsoleKey.L , "L" },
            { ConsoleKey.M , "M" },
            { ConsoleKey.N , "N" },
            { ConsoleKey.O , "O" },
            { ConsoleKey.P , "P" },
            { ConsoleKey.Q , "Q" },
            { ConsoleKey.R , "R" },
            { ConsoleKey.S , "S" },
            { ConsoleKey.T , "T" },
            { ConsoleKey.U , "U" },
            { ConsoleKey.V , "V" },
            { ConsoleKey.W , "W" },
            { ConsoleKey.X , "X" },
            { ConsoleKey.Y , "Y" },
            { ConsoleKey.Z , "Z" },
            { ConsoleKey.LeftWindows , "LeftWindows" },
            { ConsoleKey.RightWindows , "RightWindows" },
            { ConsoleKey.Applications , "Applications" },
            { ConsoleKey.Sleep , "Sleep" },
            { ConsoleKey.NumPad0 , "NumPad0" },
            { ConsoleKey.NumPad1 , "NumPad1" },
            { ConsoleKey.NumPad2 , "NumPad2" },
            { ConsoleKey.NumPad3 , "NumPad3" },
            { ConsoleKey.NumPad4 , "NumPad4" },
            { ConsoleKey.NumPad5 , "NumPad5" },
            { ConsoleKey.NumPad6 , "NumPad6" },
            { ConsoleKey.NumPad7 , "NumPad7" },
            { ConsoleKey.NumPad8 , "NumPad8" },
            { ConsoleKey.NumPad9 , "NumPad9" },
            { ConsoleKey.Multiply , "Multiply" },
            { ConsoleKey.Add , "Add" },
            { ConsoleKey.Separator , "Separator" },
            { ConsoleKey.Subtract , "Subtract" },
            { ConsoleKey.Decimal , "Decimal" },
            { ConsoleKey.Divide , "Divide" },
            { ConsoleKey.F1 , "F1" },
            { ConsoleKey.F2 , "F2" },
            { ConsoleKey.F3 , "F3" },
            { ConsoleKey.F4 , "F4" },
            { ConsoleKey.F5 , "F5" },
            { ConsoleKey.F6 , "F6" },
            { ConsoleKey.F7 , "F7" },
            { ConsoleKey.F8 , "F8" },
            { ConsoleKey.F9 , "F9" },
            { ConsoleKey.F10 , "F10" },
            { ConsoleKey.F11 , "F11" },
            { ConsoleKey.F12 , "F12" },
            { ConsoleKey.F13 , "F13" },
            { ConsoleKey.F14 , "F14" },
            { ConsoleKey.F15 , "F15" },
            { ConsoleKey.F16 , "F16" },
            { ConsoleKey.F17 , "F17" },
            { ConsoleKey.F18 , "F18" },
            { ConsoleKey.F19 , "F19" },
            { ConsoleKey.F20 , "F20" },
            { ConsoleKey.F21 , "F21" },
            { ConsoleKey.F22 , "F22" },
            { ConsoleKey.F23 , "F23" },
            { ConsoleKey.F24 , "F24" },
            { ConsoleKey.BrowserBack , "BrowserBack" },
            { ConsoleKey.BrowserForward , "BrowserForward" },
            { ConsoleKey.BrowserRefresh , "BrowserRefresh" },
            { ConsoleKey.BrowserStop , "BrowserStop" },
            { ConsoleKey.BrowserSearch , "BrowserSearch" },
            { ConsoleKey.BrowserFavorites , "BrowserFavorites" },
            { ConsoleKey.BrowserHome , "BrowserHome" },
            { ConsoleKey.VolumeMute , "VolumeMute" },
            { ConsoleKey.VolumeDown , "VolumeDown" },
            { ConsoleKey.VolumeUp , "VolumeUp" },
            { ConsoleKey.MediaNext , "MediaNext" },
            { ConsoleKey.MediaPrevious , "MediaPrevious" },
            { ConsoleKey.MediaStop , "MediaStop" },
            { ConsoleKey.MediaPlay , "MediaPlay" },
            { ConsoleKey.LaunchMail , "LaunchMail" },
            { ConsoleKey.LaunchMediaSelect , "LaunchMediaSelect" },
            { ConsoleKey.LaunchApp1 , "LaunchApp1" },
            { ConsoleKey.LaunchApp2 , "LaunchApp2" },
            { ConsoleKey.Oem1 , "Oem1" },
            { ConsoleKey.OemPlus , "OemPlus" },
            { ConsoleKey.OemComma , "OemComma" },
            { ConsoleKey.OemMinus , "OemMinus" },
            { ConsoleKey.OemPeriod , "OemPeriod" },
            { ConsoleKey.Oem2 , "Oem2" },
            { ConsoleKey.Oem3 , "Oem3" },
            { ConsoleKey.Oem4 , "Oem4" },
            { ConsoleKey.Oem5 , "Oem5" },
            { ConsoleKey.Oem6 , "Oem6" },
            { ConsoleKey.Oem7 , "Oem7" },
            { ConsoleKey.Oem8 , "Oem8" },
            { ConsoleKey.Oem102 , "Oem102" },
            { ConsoleKey.Process , "Process" },
            { ConsoleKey.Packet , "Packet" },
            { ConsoleKey.Attention , "Attention" },
            { ConsoleKey.CrSel , "CrSel" },
            { ConsoleKey.ExSel , "ExSel" },
            { ConsoleKey.EraseEndOfFile , "EraseEndOfFile" },
            { ConsoleKey.Play , "Play" },
            { ConsoleKey.Zoom , "Zoom" },
            { ConsoleKey.NoName , "NoName" },
            { ConsoleKey.Pa1 , "Pa1" },
            { ConsoleKey.OemClear , "OemClear" }
        };

        private Dictionary<string, ConsoleColor> stringToConsoleColorLookup = new Dictionary<string, ConsoleColor>()
        {
            { "black", ConsoleColor.Black },
            { "dark blue", ConsoleColor.DarkBlue },
            { "dark green", ConsoleColor.DarkGreen },
            { "dark cyan", ConsoleColor.DarkCyan },
            { "dark red", ConsoleColor.DarkRed },
            { "dark magenta", ConsoleColor.DarkMagenta },
            { "dark yellow", ConsoleColor.DarkYellow },
            { "gray", ConsoleColor.Gray },
            { "dark gray", ConsoleColor.DarkGray },
            { "blue", ConsoleColor.Blue },
            { "green", ConsoleColor.Green },
            { "cyan", ConsoleColor.Cyan },
            { "red", ConsoleColor.Red },
            { "magenta", ConsoleColor.Magenta },
            { "yellow", ConsoleColor.Yellow },
            { "white", ConsoleColor.White }
        };
        private Dictionary<ConsoleColor, string> consoleColorToStringLookup = new Dictionary<ConsoleColor, string>()
        {
            { ConsoleColor.Black, "black" },
            { ConsoleColor.DarkBlue, "dark blue" },
            { ConsoleColor.DarkGreen, "dark green" },
            { ConsoleColor.DarkCyan, "dark cyan" },
            { ConsoleColor.DarkRed, "dark red" },
            { ConsoleColor.DarkMagenta, "dark magenta" },
            { ConsoleColor.DarkYellow, "dark yellow" },
            { ConsoleColor.Gray, "gray" },
            { ConsoleColor.DarkGray, "dark gray" },
            { ConsoleColor.Blue, "blue" },
            { ConsoleColor.Green, "green" },
            { ConsoleColor.Cyan, "cyan" },
            { ConsoleColor.Red, "red" },
            { ConsoleColor.Magenta, "magenta" },
            { ConsoleColor.Yellow, "yellow" },
            { ConsoleColor.White, "white" }
        };

        public console() : base("<io <console>>") { }

        public override runtimeResult execute(item[] args)
        {
            context internalContext = base.generateContext();
            internalContext.symbolTable.set("is_key_pressed", new predefined_function("console_is_key_pressed", keyPressed, new string[4] { "key", "shift_pressed",  "control_pressed", "alt_pressed" }));
            internalContext.symbolTable.set("current_key_pressed", new predefined_function("console_current_key_pressed", anyKeyPressed, new string[0]));
            internalContext.symbolTable.set("is_numberlocked", new predefined_function("console_is_numberlocked", numberLocked, new string[0]));
            internalContext.symbolTable.set("is_capslocked", new predefined_function("console_is_capslocked", capsLocked, new string[0]));
            internalContext.symbolTable.set("get_background", new predefined_function("console_get_background", getConsoleBackground, new string[0]));
            internalContext.symbolTable.set("set_background", new predefined_function("console_set_background", setConsoleBackground, new string[1] { "color" }));
            internalContext.symbolTable.set("get_foreground", new predefined_function("console_get_foreground", getConsoleForeground, new string[0]));
            internalContext.symbolTable.set("set_foreground", new predefined_function("console_set_foreground", setConsoleForeground, new string[1] { "color" }));
            internalContext.symbolTable.set("reset_colors", new predefined_function("console_reset_colors", consoleResetColors, new string[0]));
            internalContext.symbolTable.set("get_cursor_position", new predefined_function("console_get_cursor_position", getCursorPosition, new string[0]));
            internalContext.symbolTable.set("set_cursor_position", new predefined_function("console_set_cursor_position", setCursorPosition, new string[1] { "position" }));
            internalContext.symbolTable.set("get_cursor_size", new predefined_function("console_get_cursor_size", getCursorSize, new string[0]));
            internalContext.symbolTable.set("set_cursor_size", new predefined_function("console_set_cursor_size", setCursorSize, new string[1] { "size" }));
            internalContext.symbolTable.set("get_cursor_visibility", new predefined_function("console_get_cursor_visibility", getCursorVisibility, new string[0]));
            internalContext.symbolTable.set("set_cursor_visibility", new predefined_function("console_set_cursor_visibility", setCursorVisibility, new string[1] { "visibility" }));
            internalContext.symbolTable.set("exit", new predefined_function("console_exit", stopApplication, new string[0]));

            return new runtimeResult().success(new @object(name, internalContext).setPosition(startPos, endPos).setContext(context));
        }

        private runtimeResult getCursorVisibility(context context, position[] positions)
        {
            return new runtimeResult().success(new boolean(Console.CursorVisible));
        }

        private runtimeResult setCursorVisibility(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item visibility = context.symbolTable.get("visibility");
            if (visibility is not boolean)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Visibility must be a boolean", context));

            Console.CursorVisible = ((value)visibility).storedValue;
            return result.success(new nothing());
        }

        private runtimeResult getCursorSize(context context, position[] positions)
        {
            return new runtimeResult().success(new integer(Console.CursorSize));
        }

        private runtimeResult setCursorSize(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item size = context.symbolTable.get("size");
            if (size is not integer && size is not @float)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Size must be an integer or float", context));

            int sizeValue = (int)((value)size).storedValue;
            if (sizeValue < 1 || sizeValue > 100)
                return result.failure(new runtimeError(positions[0], positions[1], RT_OVERFLOW, "Size must be in range 1-100", context));

            Console.CursorSize = sizeValue;
            return result.success(new nothing());
        }

        private runtimeResult getCursorPosition(context context, position[] positions)
        {
            (int left, int top) = Console.GetCursorPosition();

            return new runtimeResult().success(new array(new item[2] { new integer(left).setPosition(positions[0], positions[1]).setContext(context), new integer(top).setPosition(positions[0], positions[1]).setContext(context) }));
        }

        private runtimeResult setCursorPosition(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item pos = context.symbolTable.get("position");
            if (pos is not array && pos is not list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Position must be an array or list", context));

            item[] posArray = (pos is array) ? ((array)pos).storedValue : ((list)pos).storedValue.ToArray();
            if (posArray.Length != 2)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Position must be an array or list of length 2", context));

            for (int i = 0; i < posArray.Length; i++)
            {
                if (posArray[i] is not integer && posArray[i] is not @float)
                    return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Position must only contain integers or floats", context));
            }

            (int left, int top) = ((int)((value)posArray[0]).storedValue, (int)((value)posArray[1]).storedValue);
            if (left < 0 || left > Console.BufferWidth)
                return result.failure(new runtimeError(positions[0], positions[1], RT_OVERFLOW, $"Position X must be in range 0-{Console.BufferWidth}", context));
            if (top < 0 || top > Console.BufferHeight)
                return result.failure(new runtimeError(positions[0], positions[1], RT_OVERFLOW, $"Position Y must be in range 0-{Console.BufferHeight}", context));

            Console.SetCursorPosition(left, top);
            return result.success(new nothing());
        }

        private runtimeResult consoleResetColors(context context, position[] positions)
        {
            Console.ResetColor();
            return new runtimeResult().success(new nothing());
        }

        private runtimeResult stopApplication(context context, position[] positions)
        {
            Environment.Exit(0);
            return new runtimeResult().success(new nothing());
        }

        private runtimeResult getConsoleBackground(context context, position[] positions)
        {
            return new runtimeResult().success(new @string(consoleColorToStringLookup[Console.BackgroundColor]));
        }

        private runtimeResult setConsoleBackground(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item color = context.symbolTable.get("color");
            if (color is not @string && color is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Color must be a string or character_list", context));

            string colorName = (color is @string) ? ((@string)color).storedValue.ToString() : string.Join("", ((character_list)color).storedValue);
            if (!stringToConsoleColorLookup.ContainsKey(colorName))
                return result.failure(new runtimeError(positions[0], positions[1], RT_KEY, "Unknown color", context));

            Console.BackgroundColor = stringToConsoleColorLookup[colorName];
            return result.success(new nothing());
        }

        private runtimeResult getConsoleForeground(context context, position[] positions)
        {
            return new runtimeResult().success(new @string(consoleColorToStringLookup[Console.ForegroundColor]));
        }

        private runtimeResult setConsoleForeground(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item color = context.symbolTable.get("color");
            if (color is not @string && color is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Color must be a string or character_list", context));

            string colorName = (color is @string) ? ((@string)color).storedValue.ToString() : string.Join("", ((character_list)color).storedValue);
            if (!stringToConsoleColorLookup.ContainsKey(colorName))
                return result.failure(new runtimeError(positions[0], positions[1], RT_KEY, "Unknown color", context));

            Console.ForegroundColor = stringToConsoleColorLookup[colorName];
            return result.success(new nothing());
        }

        private runtimeResult keyPressed(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item key = context.symbolTable.get("key");
            if (key is not @string && key is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Key must be a string or character_list", context));

            item shift = context.symbolTable.get("shift_pressed");
            if (shift is not boolean)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Shift_pressed must be a boolean", context));

            item control = context.symbolTable.get("control_pressed");
            if (control is not boolean)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Control_pressed must be a boolean", context));

            item alt = context.symbolTable.get("alt_pressed");
            if (alt is not boolean)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Alt_pressed must be a boolean", context));

            string keyCode = (key is @string) ? ((@string)key).storedValue.ToString() : string.Join("", ((character_list)key).storedValue);
            if (!stringToKeyLookup.ContainsKey(keyCode))
                return result.failure(new runtimeError(positions[0], positions[1], RT_KEY, "Unknown key", context));

            if (!Console.KeyAvailable) return result.success(new nothing());

            ConsoleKeyInfo keyPress = Console.ReadKey(true);
            bool shiftPress = (((value)shift).storedValue) ? (keyPress.Modifiers & ConsoleModifiers.Shift) != 0 : true;
            bool controlPress = (((value)control).storedValue) ? (keyPress.Modifiers & ConsoleModifiers.Control) != 0  : true;
            bool altPress = (((value)alt).storedValue) ? (keyPress.Modifiers & ConsoleModifiers.Alt) != 0  : true;
            return result.success(new boolean(keyPress.Key == stringToKeyLookup[keyCode] && shiftPress && controlPress && altPress));
        }

        private runtimeResult anyKeyPressed(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (!Console.KeyAvailable)
                return result.success(new nothing());

            ConsoleKeyInfo keyPress = Console.ReadKey(true);

            return result.success(new array(new item[2] {
                new @string(keyToStringLookup[keyPress.Key]).setPosition(positions[0], positions[1]).setContext(context),
                new array(new item[3] {
                        new boolean((keyPress.Modifiers & ConsoleModifiers.Shift) != 0).setPosition(positions[0], positions[1]).setContext(context),
                        new boolean((keyPress.Modifiers & ConsoleModifiers.Control) != 0).setPosition(positions[0], positions[1]).setContext(context),
                        new boolean((keyPress.Modifiers & ConsoleModifiers.Alt) != 0).setPosition(positions[0], positions[1]).setContext(context)
                    }).setPosition(positions[0], positions[1]).setContext(context)
            }));
        }

        private runtimeResult numberLocked(context context, position[] positions)
        {
            return new runtimeResult().success(new boolean(Console.NumberLock));
        }

        private runtimeResult capsLocked(context context, position[] positions)
        {
            return new runtimeResult().success(new boolean(Console.CapsLock));
        }

        public override item copy() { return new console().setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<builtin library {name}>"; }
        public override bool ItemEquals(item obj, out error? error) { error = null; return obj is console; }
    }

    public class @file : baseFunction
    {
        public @file() : base("<io <file>>") { }

        public override runtimeResult execute(item[] args)
        {
            context internalContext = base.generateContext();
            internalContext.symbolTable.set("exists", new predefined_function("file_exists", fileExists, new string[1] { "filepath" }));
            internalContext.symbolTable.set("create", new predefined_function("file_create", createFile, new string[1] { "filepath" }));
            internalContext.symbolTable.set("delete", new predefined_function("file_delete", deleteFile, new string[1] { "filepath" }));
            internalContext.symbolTable.set("read", new predefined_function("file_read", readFile, new string[1] { "filepath" }));
            internalContext.symbolTable.set("write", new predefined_function("file_write", writeFile, new string[3] { "contents", "filepath", "mode" }));
            internalContext.symbolTable.set("copy", new predefined_function("file_copy", copyFile, new string[2] { "from_path", "to_path" }));
            internalContext.symbolTable.set("move", new predefined_function("file_move", moveFile, new string[2] { "from_path", "to_path" }));

            return new runtimeResult().success(new @object(name, internalContext).setPosition(startPos, endPos).setContext(context));
        }

        private runtimeResult fileExists(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("filepath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Filepath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            return result.success(new boolean(File.Exists(filepath)));
        }

        private runtimeResult createFile(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("filepath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Filepath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);

            try
            {
                File.Create(filepath).Close();
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to create file \"{filepath}\"\n{exception.Message}", context));
            }

            return result.success(new nothing());
        }

        private runtimeResult deleteFile(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("filepath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Filepath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            if (!File.Exists(filepath))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"File \"{filepath}\" does not exist", context));

            try
            {
                File.Delete(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to delete file \"{filepath}\"\n{exception.Message}", context));
            }

            return result.success(new nothing());
        }

        private runtimeResult readFile(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("filepath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Filepath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            if (!File.Exists(filepath))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"File \"{filepath}\" does not exist", context));

            string[] contents;
            try
            {
                contents = File.ReadAllLines(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to load file \"{filepath}\"\n{exception.Message}", context));
            }

            item[] contentsAsString = new item[contents.Length];
            for (int i = 0; i < contentsAsString.Length; i++)
                contentsAsString[i] = new @string(contents[i]).setPosition(positions[0], positions[1]).setContext(context);
            return result.success(new array(contentsAsString));
        }

        private runtimeResult writeFile(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item content = context.symbolTable.get("contents");
            if (content is not @string && content is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Contents must be a string or character_list", context));
            item path = context.symbolTable.get("filepath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Filepath must be a string or character_list", context));
            item mode = context.symbolTable.get("mode");
            if (mode is not @string && mode is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Mode must be a string or character_list", context));

            string content_ = (content is @string) ? ((@string)content).storedValue.ToString() : string.Join("", ((character_list)content).storedValue);
            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            string mode_ = ((mode is @string) ? ((@string)mode).storedValue.ToString() : string.Join("", ((character_list)mode).storedValue)).ToLower();

            if (mode_ == "write")
            {
                try
                {
                    File.WriteAllText(filepath, content_);
                }
                catch (IOException exception)
                {
                    return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to write to file \"{filepath}\"\n{exception.Message}", context));
                }
            }
            else if (mode_ == "append")
            {
                try
                {
                    File.AppendAllText(filepath, content_);
                }
                catch (IOException exception)
                {
                    return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to append to file \"{filepath}\"\n{exception.Message}", context));
                }
            }
            else
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Mode must be string/character_list literal \"write\" or \"append\"", context));

            return result.success(new nothing());
        }

        private runtimeResult copyFile(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item from = context.symbolTable.get("from_path");
            if (from is not @string && from is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "From_path must be a string or character_list", context));
            item to = context.symbolTable.get("to_path");
            if (to is not @string && to is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "To_path must be a string or character_list", context));

            string from_ = (from is @string) ? ((@string)from).storedValue.ToString() : string.Join("", ((character_list)from).storedValue);
            string to_ = (to is @string) ? ((@string)to).storedValue.ToString() : string.Join("", ((character_list)to).storedValue);

            if (!File.Exists(from_))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"File \"{from_}\" does not exist", context));

            try
            {
                File.Copy(from_, to_);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to copy contents of file \"{from_}\" to \"{to_}\"\n{exception.Message}", context));
            }

            return result.success(new nothing());
        }

        private runtimeResult moveFile(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item from = context.symbolTable.get("from_path");
            if (from is not @string && from is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "From_path must be a string or character_list", context));
            item to = context.symbolTable.get("to_path");
            if (to is not @string && to is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "To_path must be a string or character_list", context));

            string from_ = (from is @string) ? ((@string)from).storedValue.ToString() : string.Join("", ((character_list)from).storedValue);
            string to_ = (to is @string) ? ((@string)to).storedValue.ToString() : string.Join("", ((character_list)to).storedValue);

            if (!File.Exists(from_))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"File \"{from_}\" does not exist", context));

            try
            {
                File.Move(from_, to_);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to move file \"{from_}\" to \"{to_}\"\n{exception.Message}", context));
            }

            return result.success(new nothing());
        }

        public override item copy() { return new @file().setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<builtin library {name}>"; }
        public override bool ItemEquals(item obj, out error? error) { error = null; return obj is @file; }
    }

    public class folder : baseFunction
    {
        public folder() : base("<io <folder>>") { }

        public override runtimeResult execute(item[] args)
        {
            context internalContext = base.generateContext();
            internalContext.symbolTable.set("exists", new predefined_function("folder_exists", folderExists, new string[1] { "folderpath" }));
            internalContext.symbolTable.set("create", new predefined_function("folder_create", createFolder, new string[1] { "folderpath" }));
            internalContext.symbolTable.set("delete", new predefined_function("folder_delete", deleteFolder, new string[1] { "folderpath" }));
            internalContext.symbolTable.set("subfolders_in", new predefined_function("folder_subfolders_in", subFolders, new string[1] { "folderpath" }));
            internalContext.symbolTable.set("files_in", new predefined_function("folder_files_in", filesInFolder, new string[1] { "folderpath" }));
            internalContext.symbolTable.set("files_and_subfolders_in", new predefined_function("folder_files_and_subfolders_in", filesAndSubFolders, new string[1] { "folderpath" }));
            internalContext.symbolTable.set("parent_of", new predefined_function("folder_parent_of", folderParent, new string[1] { "folderpath" }));
            internalContext.symbolTable.set("root_of", new predefined_function("folder_root_of", folderRoot, new string[1] { "folderpath" }));
            internalContext.symbolTable.set("current", new predefined_function("folder_current", currentFolder, new string[0]));
            internalContext.symbolTable.set("set_current", new predefined_function("folder_set_current", setCurrentFolder, new string[1] { "folderpath" }));
            internalContext.symbolTable.set("move", new predefined_function("folder_move", moveFolder, new string[2] { "from_path", "to_path" }));

            return new runtimeResult().success(new @object(name, internalContext).setPosition(startPos, endPos).setContext(context));
        }

        private runtimeResult folderExists(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("folderpath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Folderpath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            return result.success(new boolean(Directory.Exists(filepath)));
        }

        private runtimeResult createFolder(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("folderpath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Folderpath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);

            try
            {
                Directory.CreateDirectory(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to create folder \"{filepath}\"\n{exception.Message}", context));
            }

            return result.success(new nothing());
        }

        private runtimeResult deleteFolder(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("folderpath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Folderpath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            if (!Directory.Exists(filepath))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Folder \"{filepath}\" does not exist", context));

            try
            {
                Directory.Delete(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to delete folder \"{filepath}\"\n{exception.Message}", context));
            }

            return result.success(new nothing());
        }

        private runtimeResult subFolders(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("folderpath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Folderpath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            if (!Directory.Exists(filepath))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Folder \"{filepath}\" does not exist", context));

            string[] subDirectories;
            try
            {
                subDirectories = Directory.GetDirectories(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to access subfolders in folder \"{filepath}\"\n{exception.Message}", context));
            }

            item[] subDirectoriesAsString = new item[subDirectories.Length];
            for (int i = 0; i < subDirectories.Length; i++)
                subDirectoriesAsString[i] = new @string(subDirectories[i]).setPosition(positions[0], positions[1]).setContext(context);
            return result.success(new array(subDirectoriesAsString));
        }
        
        private runtimeResult filesInFolder(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("folderpath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Folderpath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            if (!Directory.Exists(filepath))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Folder \"{filepath}\" does not exist", context));

            string[] subFiles;
            try
            {
                subFiles = Directory.GetFiles(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to access files in folder \"{filepath}\"\n{exception.Message}", context));
            }

            item[] subFilesAsString = new item[subFiles.Length];
            for (int i = 0; i < subFiles.Length; i++)
                subFilesAsString[i] = new @string(subFiles[i]).setPosition(positions[0], positions[1]).setContext(context);
            return result.success(new array(subFilesAsString));
        }

        private runtimeResult filesAndSubFolders(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("folderpath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Folderpath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            if (!Directory.Exists(filepath))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Folder \"{filepath}\" does not exist", context));

            string[] subFilesAndDirectories;
            try
            {
                subFilesAndDirectories = Directory.GetFileSystemEntries(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to access subfolders and files in folder \"{filepath}\"\n{exception.Message}", context));
            }

            item[] subFilesAndDirectoriesAsString = new item[subFilesAndDirectories.Length];
            for (int i = 0; i < subFilesAndDirectories.Length; i++)
                subFilesAndDirectoriesAsString[i] = new @string(subFilesAndDirectories[i]).setPosition(positions[0], positions[1]).setContext(context);
            return result.success(new array(subFilesAndDirectoriesAsString));
        }

        private runtimeResult folderParent(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("folderpath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Folderpath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            if (!Directory.Exists(filepath))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Folder \"{filepath}\" does not exist", context));

            DirectoryInfo? parentInfo;
            try
            {
                parentInfo = Directory.GetParent(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to access parent of folder \"{filepath}\"\n{exception.Message}", context));
            }

            return result.success((parentInfo != null) ? new @string(parentInfo.Name) : new nothing());
        }

        private runtimeResult folderRoot(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("folderpath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Folderpath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            if (!Directory.Exists(filepath))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Folder \"{filepath}\" does not exist", context));

            string root;
            try
            {
                root = Directory.GetDirectoryRoot(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to access root of folder \"{filepath}\"\n{exception.Message}", context));
            }

            return result.success(new @string(root));
        }

        private runtimeResult moveFolder(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item from = context.symbolTable.get("from_path");
            if (from is not @string && from is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "From_path must be a string or character_list", context));
            item to = context.symbolTable.get("to_path");
            if (to is not @string && to is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "To_path must be a string or character_list", context));

            string from_ = (from is @string) ? ((@string)from).storedValue.ToString() : string.Join("", ((character_list)from).storedValue);
            string to_ = (to is @string) ? ((@string)to).storedValue.ToString() : string.Join("", ((character_list)to).storedValue);

            if (!Directory.Exists(from_))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Folder \"{from_}\" does not exist", context));

            try
            {
                Directory.Move(from_, to_);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to move folder \"{from_}\" to \"{to_}\"\n{exception.Message}", context));
            }

            return result.success(new nothing());
        }

        private runtimeResult currentFolder(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            string filepath;
            try
            {
                filepath = Directory.GetCurrentDirectory();
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to access current working directory\n{exception.Message}", context));
            }

            return result.success(new @string(filepath));
        }

        private runtimeResult setCurrentFolder(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("folderpath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Folderpath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            if (!Directory.Exists(filepath))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Folder \"{filepath}\" does not exist", context));

            try
            {
                Directory.SetCurrentDirectory(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to set current working directory to \"{filepath}\"\n{exception.Message}", context));
            }

            return result.success(new nothing());
        }

        public override item copy() { return new folder().setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<builtin library {name}>"; }
        public override bool ItemEquals(item obj, out error? error) { error = null; return obj is folder; }
    }

    public class path : baseFunction
    {
        public path() : base("<io <path>>") { }

        public override runtimeResult execute(item[] args)
        {
            context internalContext = base.generateContext();

            internalContext.symbolTable.set("directory_separator", new @string(Path.DirectorySeparatorChar.ToString()));
            internalContext.symbolTable.set("alternate_directory_separator", new @string(Path.AltDirectorySeparatorChar.ToString()));
            internalContext.symbolTable.set("invalid_filename_characters", new @string(string.Join("", Path.GetInvalidFileNameChars())));
            internalContext.symbolTable.set("invalid_path_characters", new @string(string.Join("", Path.GetInvalidPathChars())));
            internalContext.symbolTable.set("exists", new predefined_function("path_exists", pathExists, new string[1] { "path" }));
            internalContext.symbolTable.set("join", new predefined_function("path_join", joinPaths, new string[1] { "paths" }));
            internalContext.symbolTable.set("combine", new predefined_function("path_combine", combinePaths, new string[2] { "path_1", "path_2" }));
            internalContext.symbolTable.set("has_extension", new predefined_function("path_has_extension", pathHasExtension, new string[1] { "path" }));
            internalContext.symbolTable.set("get_extension", new predefined_function("path_get_extension", pathExtension, new string[1] { "path" }));
            internalContext.symbolTable.set("set_extension", new predefined_function("path_set_extension", setPathExtension, new string[2] { "path", "extension" }));
            internalContext.symbolTable.set("get_folder", new predefined_function("path_get_folder", folderNameOfPath, new string[1] { "path" }));
            internalContext.symbolTable.set("get_file", new predefined_function("path_get_file", fileNameOfPath, new string[1] { "path" }));
            internalContext.symbolTable.set("get_file_without_extension", new predefined_function("path_get_file_without_extension", fileNameOfPathWithoutExtension, new string[1] { "path" }));
            internalContext.symbolTable.set("get_whole", new predefined_function("path_get_whole", fullPath, new string[1] { "path" }));
            internalContext.symbolTable.set("get_root", new predefined_function("path_get_root", pathRoot, new string[1] { "path" }));
            internalContext.symbolTable.set("create_temp_file", new predefined_function("path_create_temp_file", createTempFilePath, new string[0]));
            internalContext.symbolTable.set("temp", new predefined_function("path_temp", tempPath, new string[0]));
            internalContext.symbolTable.set("relative_path", new predefined_function("path_relative_path", relativePath, new string[2] { "relative_to", "path" }));
            internalContext.symbolTable.set("ends_in_folder_seperator", new predefined_function("path_ends_in_folder_seperator", pathEndsInFolderSeperator, new string[1] { "path" }));
            internalContext.symbolTable.set("remove_last_folder_seperator", new predefined_function("path_remove_last_folder_seperator", removeLastFolderSeperatorOfPath, new string[1] { "path" }));

            return new runtimeResult().success(new @object(name, internalContext).setPosition(startPos, endPos).setContext(context));
        }

        private runtimeResult pathExists(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            return result.success(new boolean(Path.Exists(filepath)));
        }

        private runtimeResult joinPaths(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item paths = context.symbolTable.get("paths");
            if (paths is not list && paths is not array)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Paths must be a list or array", context));

            item[] paths_ = (paths is list) ? ((list)paths).storedValue.ToArray() : ((array)paths).storedValue;
            string[] filepaths = new string[paths_.Length];
            for (int i = 0; i < paths_.Length; i++)
            {
                if (paths_[i] is not @string && paths_[i] is not character_list)
                    return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "All elements of paths must be strings or character_lists", context));
                filepaths[i] = (paths_[i] is @string) ? ((@string)paths_[i]).storedValue : string.Join("", ((character_list)paths_[i]).storedValue);
            }

            return result.success(new @string(Path.Join(filepaths)));
        }

        private runtimeResult combinePaths(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path1 = context.symbolTable.get("path_1");
            if (path1 is not @string && path1 is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path_1 must be a string or character_list", context));
            item path2 = context.symbolTable.get("path_2");
            if (path2 is not @string && path2 is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path_2 must be a string or character_list", context));

            string filepath1 = (path1 is @string) ? ((@string)path1).storedValue.ToString() : string.Join("", ((character_list)path1).storedValue);
            string filepath2 = (path2 is @string) ? ((@string)path2).storedValue.ToString() : string.Join("", ((character_list)path2).storedValue);
            return result.success(new @string(Path.Combine(filepath1, filepath2)));
        }

        private runtimeResult pathHasExtension(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            return result.success(new boolean(Path.HasExtension(filepath)));
        }

        private runtimeResult pathExtension(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);

            string? extension = Path.GetExtension(filepath);
            return result.success((extension != null) ? new @string(extension) : new nothing());
        }

        private runtimeResult setPathExtension(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));
            item extension = context.symbolTable.get("extension");
            if (extension is not @string && extension is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Extension must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            string extension_ = (extension is @string) ? ((@string)extension).storedValue.ToString() : string.Join("", ((character_list)extension).storedValue);

            string? newPath = Path.ChangeExtension(filepath, extension_);
            return result.success((newPath != null) ? new @string(newPath) : new nothing());
        }

        private runtimeResult folderNameOfPath(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);

            string? folder;
            try
            {
                folder = Path.GetDirectoryName(filepath);
            }
            catch (Exception exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Could not access directory name of path \"{filepath}\"\n\n{exception.Message}", context));
            }

            return result.success((folder != null) ? new @string(folder) : new nothing());
        }

        private runtimeResult fileNameOfPath(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);

            string? filename;
            try
            {
                filename = Path.GetFileName(filepath);
            }
            catch (Exception exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Could not access file name of path \"{filepath}\"\n\n{exception.Message}", context));
            }

            return result.success((filename != null) ? new @string(filename) : new nothing());
        }

        private runtimeResult fileNameOfPathWithoutExtension(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);

            string? filename;
            try
            {
                filename = Path.GetFileNameWithoutExtension(filepath);
            }
            catch (Exception exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Could not access file name of path \"{filepath}\"\n\n{exception.Message}", context));
            }

            return result.success((filename != null) ? new @string(filename) : new nothing());
        }

        private runtimeResult fullPath(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);

            string filepath_;
            try
            {
                filepath_ = Path.GetFullPath(filepath);
            }
            catch (Exception exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Could not access full path of path \"{filepath}\"\n\n{exception.Message}", context));
            }

            return result.success(new @string(filepath_));
        }

        private runtimeResult pathRoot(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);

            string? filepath_;
            try
            {
                filepath_ = Path.GetPathRoot(filepath);
            }
            catch (Exception exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Could not access root of path \"{filepath}\"\n\n{exception.Message}", context));
            }

            return result.success((filepath_ != null) ? new @string(filepath_) : new nothing());
        }

        private runtimeResult createTempFilePath(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            string filepath;
            try
            {
                filepath = Path.GetTempFileName();
            }
            catch (Exception exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Could not create new temporary file\n\n{exception.Message}", context));
            }

            return result.success(new @string(filepath));
        }

        private runtimeResult tempPath(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            string filepath;
            try
            {
                filepath = Path.GetTempPath();
            }
            catch (Exception exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Could not access Temp path\n\n{exception.Message}", context));
            }

            return result.success(new @string(filepath));
        }

        private runtimeResult relativePath(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item relativeTo = context.symbolTable.get("relative_to");
            if (relativeTo is not @string && relativeTo is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Relative_to must be a string or character_list", context));
            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string relativeTo_ = (relativeTo is @string) ? ((@string)relativeTo).storedValue.ToString() : string.Join("", ((character_list)relativeTo).storedValue);
            string path_ = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);

            return result.success(new @string(Path.GetRelativePath(relativeTo_, path_)));
        }

        private runtimeResult pathEndsInFolderSeperator(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            return result.success(new boolean(Path.EndsInDirectorySeparator(filepath)));
        }

        private runtimeResult removeLastFolderSeperatorOfPath(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            return result.success(new @string(Path.TrimEndingDirectorySeparator(filepath)));
        }

        public override item copy() { return new path().setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<builtin library {name}>"; }
        public override bool ItemEquals(item obj, out error? error) { error = null; return obj is path; }
    }
}