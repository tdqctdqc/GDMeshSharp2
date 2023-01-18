using Godot;
using System;
using System.Text.Json;

public class ProcedureUpdate : IUpdate
{
    string IUpdate.UpdateType => UpdateType;
    public static string UpdateType = "Procedure";
    public string ProcedureName { get; private set; }
    public string ArgsJson { get; private set; }

    public ProcedureUpdate(string procedureName, string argsJson)
    {
        ProcedureName = procedureName;
        ArgsJson = argsJson;
    }
    public string Serialize()
    {
        return JsonSerializer.Serialize(this);
    }

    public static void DeserializeAndEnact(string json, ServerWriteKey key)
    {
        var update = JsonSerializer.Deserialize<ProcedureUpdate>(json);
        ProcedureMeta.TriggerProcedure(update.ProcedureName, update.ArgsJson, key);
    }
}
