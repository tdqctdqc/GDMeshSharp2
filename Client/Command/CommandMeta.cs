using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

public static class CommandMeta
{
    private static Dictionary<string, Action<string, HostWriteKey>> _enacts; 
    public static void Setup()
    {
        _enacts = new Dictionary<string, Action<string, HostWriteKey>>();
        var procTypes = Assembly.GetExecutingAssembly().GetConcreteTypesOfType<ICommand>();
        foreach (var procType in procTypes)
        {
            var enactMi = procType.GetMethod("DeserializeAndEnact", BindingFlags.Static | BindingFlags.Public);
            var del = enactMi.MakeStaticMethodDelegate<Action<string, HostWriteKey>>();
            _enacts.Add(procType.Name, del);
        }
    }

    public static void Enact(HostWriteKey key, string commandName, string commandJson)
    {
        _enacts[commandName](commandJson, key);
    }
}
