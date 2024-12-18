using System;
using System.Text;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Server;
using SirhurtRConsole.Luau;
using System.Linq;

class Program
{
    /*
    
     -- Connect to the WebSocket server
local ws = WebSocket.connect("ws://127.0.0.1:8080/sirhurt-func")

-- Store functions in getgenv().sirhurt
getgenv().sirhurt = {}

-- Handle incoming messages from the server
ws.OnMessage:Connect(function(message)
    print("Received message from server: " .. message)

    -- Check if the message is a function registration
    if message:match("register:") then
        local funcName = message:match("register:(.*)")
        
        -- Register the function in getgenv().sirhurt
        getgenv().sirhurt[funcName] = function(...)
            -- Send the function call to the server
            local args = table.concat({...}, ",")
            ws:Send(funcName .. ":args|" .. args .. "|nil")
        end
        
        print("Registered function: " .. funcName)
    end

    if message:match(":ret|") then
        local funcName = message:match("^(.-):ret|")
        local result = message:match(":ret|'(.-)'")
        if funcName and result then
            --print("Result from server for function " .. funcName .. ": " .. result)
        end
    end
end)

ws.OnClose:Connect(function()
    print("WebSocket connection closed.")
end)

ws:Send("get_registered_functions")

wait(1)
sirhurt.consoleprint("hello from roblox!!")
     
     */
    static WebSocketServer wssv;

    static void Main(string[] args)
    {
        InitConnection(); // allow sirhurt to connect and inherit custom functions

        RegisterLuauFunc("consoleprint", ScriptContext.rconsoleprint);
        RegisterLuauFunc("rconsoleprint", ScriptContext.rconsoleprint);

        Console.ReadLine(); // 
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
        public static readonly Dictionary<string, Func<return_state, string>> RegisteredFunctions = new();

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
                string ArgRetCount = ProcessFunctionCall(funcState); // result is how many arguments to return

                if (funcState.lua_gettop() == 1 && funcState.lua_iserror(0))
                {
                    // exampleFunc:error:More or less then 1 string argument
                    Send($"{funcState.FuncName}:{funcState.Arguments[0]}");
                    return;
                }

                // exampleFunc:ret,2|'sex'|'sex2'
                Send($"{funcState.FuncName}:ret,{ArgRetCount}|'{string.Join("|", funcState.Return)}'");
            }
        }

        private string ProcessFunctionCall(return_state state)
        {
            // Check if the function is registered
            if (RegisteredFunctions.TryGetValue(state.FuncName, out var func))
                return func(state);
            else return $"Function '{state.FuncName}' not registered.";
        }

        private void SendRegisteredFunctions()
        {
            foreach (var funcName in RegisteredFunctions.Keys)
                Send($"register:{funcName}");
        }
    }
}