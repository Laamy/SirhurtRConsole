using System;

namespace SirhurtRConsole.Luau;
internal class ScriptContext
{
    public static int rconsoleprint(return_state state)
    {
        if (state.lua_gettop() != 1)
            return state.lua_error("More or less then 1 string argument");

        if (!state.lua_isstring(0))
            return state.lua_error("Argument must be a string");

        Console.Write(state.lua_tostring(0)); // no colour codes yet

        return 0;
    }
}
