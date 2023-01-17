using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

public static class ProcedureMeta
{
    private static Dictionary<string, Action<ServerWriteKey, string>> _deserializeAndEnacts;

    public static void Setup()
    {
        _deserializeAndEnacts = new Dictionary<string, Action<ServerWriteKey, string>>();
        var procTypes = Assembly.GetExecutingAssembly().GetConcreteTypesOfType<Procedure>();
        foreach (var procType in procTypes)
        {
            var name = procType.Name;
            var deserializeEnactMi = procType.GetMethod(nameof(ExampleProcedure.ReceiveFromServer),
                BindingFlags.Static | BindingFlags.Public);
            var del = deserializeEnactMi.MakeStaticMethodDelegate<Action<ServerWriteKey, string>>();
            _deserializeAndEnacts.Add(name, del);
        }
    }

    public static void TriggerProcedure(string procName, string procArgsJson, ServerWriteKey key)
    {
        _deserializeAndEnacts[procName](key, procArgsJson);
    }
}
