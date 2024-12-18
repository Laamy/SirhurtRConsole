using System.Text.RegularExpressions;

namespace SirhurtRConsole.Luau;
internal class return_state
{
    /*
    
public static int rconsoleprint(lua_state state)
{
    // something like lua_gettop(state) != 1 return lua_error(state, "more or less then 1 argument!!")
    // something like !lua_isstring(state, 1) return lua_error(state, "expected string got 'junk'")

    // then console.write(state, lua_tostring(state, 1)

    return 0; // how many results to return..
}
     
     */

    public string RawData { get; set; }
    public string FuncName { get; set; }
    public string[] Arguments { get; set; }

    public string[] Return { get; set; } = null;

    public return_state(string rawData)
    {
        RawData = rawData;

        FuncName = RawData.Split(':')[0];
        var argsString = RawData.Split(':')[1].Split('|')[1];
        Arguments = argsString.Split(',');
    }

    // variables TODO:
    // nil
    // booleans (true false)
    // numbers
    // strings
    // tables (i might drop this for now..)
    // functions (i might drop this for now..)
    // user data threads ect.. (i might drop this for now..)

    public  bool lua_isstring(int index) => index >= 1 && index <= Arguments.Length && Arguments[index - 1].StartsWith("'") && Arguments[index - 1].EndsWith("'");
    public  string lua_tostring(int index) => Arguments[index - 1].Trim('\'');

    public  bool lua_isnil(int index) => Arguments[index] == "nil";

    public  bool lua_isbool(int index) => (Arguments[index] == "true" || Arguments[index] == "false");
    public  bool lua_tobool(int index) => Arguments[index] == "true" ? true : false; // lazy solution

    public  bool lua_isnumber(int index) => double.TryParse(Arguments[index], out _);
    public  double lua_tonumber(int index) => double.Parse(Arguments[index]);

    public bool lua_iserror(int index) => Regex.IsMatch(Arguments[index], @"error:\\(.*?\\)");

    public int lua_gettop() => Arguments.Length;
    public int lua_error(string err)
    {
        Return = [$"error:{err}"];
        return 1;
    }
}
