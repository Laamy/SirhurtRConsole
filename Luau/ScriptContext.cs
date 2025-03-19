using System;
using System.Runtime.InteropServices;

namespace SirhurtRConsole.Luau;

internal class ScriptContext
{
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow); // might use C++ CLR instead..

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    public static int rconsoleprint(return_state state)
    {
        if (state.lua_gettop() != 1)
            state.lua_error("Argument 1 should be a string value");

        var data = state.lua_checkstring(1);

        switch (data)
        {
            case "@@BLACK@@":
                Console.ForegroundColor = ConsoleColor.Black;
                break;
            case "@@BLUE@@":
                Console.ForegroundColor = ConsoleColor.Blue;
                break;
            case "@@GREEN@@":
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            case "@@CYAN@@":
                Console.ForegroundColor = ConsoleColor.Cyan;
                break;
            case "@@RED@@":
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case "@@MAGENTA@@":
                Console.ForegroundColor = ConsoleColor.Magenta;
                break;
            case "@@BROWN@@":
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                break;
            case "@@LIGHT_GRAY@@":
                Console.ForegroundColor = ConsoleColor.Gray;
                break;
            case "@@DARK_GRAY@@":
                Console.ForegroundColor = ConsoleColor.DarkGray;
                break;
            case "@@LIGHT_BLUE@@":
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                break;
            case "@@LIGHT_GREEN@@":
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                break;
            case "@@LIGHT_CYAN@@":
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                break;
            case "@@LIGHT_RED@@":
                Console.ForegroundColor = ConsoleColor.DarkRed;
                break;
            case "@@LIGHT_MAGENTA@@":
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                break;
            case "@@YELLOW@@":
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case "@@WHITE@@":
                Console.ForegroundColor = ConsoleColor.White;
                break;
            default:
                Console.Write(data);
                break;
        }

        return 0;
    }

    public static int rconsoleclear(return_state state)
    {
        Console.Clear();

        return 0;
    }

    public static int rconsolecreate(return_state state)
    {
        //show the console via show-window
        ShowWindow(GetConsoleWindow(), 1);

        return 0;
    }

    public static int rconsoledestroy(return_state state)
    {
        ShowWindow(GetConsoleWindow(), 0);

        return 0;
    }

    public static int rconsoleinput(return_state state)
    {
        var input = Console.ReadLine();

        if (string.IsNullOrEmpty(input))
            state.Return = ["nil"]; //state.lua_error("User provided empty input");
        else state.Return = [$"{input}"];

        return 1;
    }

    public static int rconsolesettitle(return_state state)
    {
        if (state.lua_gettop() != 1)
            state.lua_error("Argument 1 should be a string value");

        var title = state.lua_checkstring(1);
        Console.Title = title;

        return 0;
    }
}
