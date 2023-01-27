using Godot;
using System;
using System.Text.Json;

public class ProcedureUpdate : Update
{
    public string ProcedureName { get; private set; }
    public object[] ProcedureArgs { get; private set; }

    public ProcedureUpdate(string procedureName, object[] procedureArgs, HostWriteKey key) : base(key)
    {
        ProcedureArgs = procedureArgs;
        ProcedureName = procedureName;
    }
    public string Serialize()
    {
        return Game.I.Serializer.Serialize(this);
    }
    public override void Enact(ServerWriteKey key)
    {
        var meta = Game.I.Serializer.GetProcedureMeta(ProcedureName);
        //todo revert
        // var proc = meta.Deserialize(ProcedureArgs);
        // proc.Enact(key);
    }

    private static ProcedureUpdate DeserializeConstructor(object[] args)
    {
        return new ProcedureUpdate(args);
    }
    private ProcedureUpdate(object[] args) : base(args) {}
}
