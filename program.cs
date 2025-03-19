using System;
using System.Text;
using System.Collections.Generic;

using WebSocketSharp;
using WebSocketSharp.Server;

using SirhurtRConsole.Luau;
using System.Threading;

class Program
{
    /*
    
local ws = WebSocket.connect("ws://127.0.0.1:8080/sirhurt-func")

getgenv().sirhurt = {}

local functionsWaitingForReturn = {}

ws.OnMessage:Connect(function(message)
    --secureinfo("Received message from server: " .. message)

    if message:match("register:") then
        local funcName = message:match("register:(.*)")
        
        getgenv().sirhurt[funcName] = function(...)
            local formattedArgs = {}
        
            for _, v in ipairs({...}) do
                if type(v) == "string" then
                    table.insert(formattedArgs, "'" .. v .. "'")
                else
                    table.insert(formattedArgs, tostring(v))
                end
            end
        
            --secureinfo(funcName .. ":args|" .. table.concat(formattedArgs, ",") .. "|nil")
            ws:Send(funcName .. ":args|" .. table.concat(formattedArgs, ",") .. "|nil")

            functionsWaitingForReturn[funcName] = {isDone = false}
            repeat task.wait() until functionsWaitingForReturn[funcName].isDone
            return unpack(functionsWaitingForReturn[funcName].ret)
        end
        
        --secureinfo("Registered function: " .. funcName)
    end

    -- only :ret, is alwways there
    -- funcname:ret,argsCount|arg1|arg2
    if message:match(":ret,") then
        local funcName = message:match("(.+):ret")
        local argsCount = message:match("ret,(%d+)|")
        local args = message:match("|(.+)")
        local argTable = {}
        
        -- error:{err} is the first argument if an error happened
        if #args == 1 and args:match("error:") then
            error(args:match("error:(.*)"))
        end

        for arg in args:gmatch("[^,]+") do
            if arg == "nil" then
                table.insert(argTable, nil)
            elseif arg == "true" then
                table.insert(argTable, true)
            elseif arg == "false" then
                table.insert(argTable, false)
            elseif arg:match("^'") and arg:match("'$") then
                table.insert(argTable, arg:sub(2, -2))
            elseif tonumber(arg) then
                table.insert(argTable, tonumber(arg))
            else
                error("Invalid argument type: " .. arg)--wont ever trigger
            end
        end
        
        functionsWaitingForReturn[funcName].ret = argTable
        functionsWaitingForReturn[funcName].isDone = true
    end
end)

ws.OnClose:Connect(function()
    secureinfo("WebSocket connection closed.")
end)

ws:Send("get_registered_functions")

task.wait(0.5)
sirhurt.rconsoledestroy()
     
     */
    static WebSocketServer wssv;

    static void Main(string[] args)
    {
        InitConnection(); // allow sirhurt to connect and inherit custom functions

        RegisterLuauFunc("rconsolecreate", ScriptContext.rconsolecreate);
        RegisterLuauFunc("rconsoledestroy", ScriptContext.rconsoledestroy);
        RegisterLuauFunc("rconsoleprint", ScriptContext.rconsoleprint);
        RegisterLuauFunc("rconsolesettitle", ScriptContext.rconsolesettitle);
        RegisterLuauFunc("rconsoleclear", ScriptContext.rconsoleclear);
        RegisterLuauFunc("rconsoleinput", ScriptContext.rconsoleinput);

        // keep console open forever
        while (true) { } // CPU..
    }

    static void InitConnection()
    {
        wssv = new WebSocketServer("ws://127.0.0.1:8080");
        wssv.AddWebSocketService<LuaWebSocketBehavior>("/sirhurt-func");
        wssv.Start();
    }

    public static void RegisterLuauFunc(string funcName, Func<return_state, int> func)
    {
        LuaWebSocketBehavior.RegisteredFunctions[funcName] = func;
    }

    public class LuaWebSocketBehavior : WebSocketBehavior
    {
        public static readonly Dictionary<string, Func<return_state, int>> RegisteredFunctions = new();

        protected override void OnMessage(MessageEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.RawData);
            //Console.WriteLine($"Received message: {message}");

            if (message == "get_registered_functions")
            {
                SendRegisteredFunctions();
            }

            if (message.Contains(":args|"))
            {
                return_state funcState = new return_state(message);
                try
                {
                    int ArgRetCount = ProcessFunctionCall(funcState); // result is how many arguments to return

                    if (funcState.lua_gettop() == 1 && funcState.lua_iserror(0))
                    {
                        // exampleFunc:error:More or less then 1 string argument
                        Send($"{funcState.FuncName}:{funcState.Arguments[0]}");
                        return;
                    }

                    // exampleFunc:ret,2|'sex'|'sex2'
                    var statement = (funcState.Return != null && funcState.Return.Length > 0) ? string.Join("|", funcState.Return) : "";
                    Send($"{funcState.FuncName}:ret,{ArgRetCount}|'{statement}'");
                }
                catch (Exception ex)
                {
                    Send($"{funcState.FuncName}:ret,{0}|'{ex.Message}'");
                }
            }
        }

        private int ProcessFunctionCall(return_state state)
        {
            // Check if the function is registered
            if (RegisteredFunctions.TryGetValue(state.FuncName, out var func))
                return func(state);
            else state.lua_error($"Function '{state.FuncName}' not registered.");
            state.lua_error("Function not found.");
            return 0; // this wont matter but msvc sobs about it
        }

        private void SendRegisteredFunctions()
        {
            foreach (var funcName in RegisteredFunctions.Keys)
            {
                // sirhurts websockets can lose data
                Thread.Sleep(1);//this is actualy 64hz..
                Send($"register:{funcName}");
            }
        }
    }
}