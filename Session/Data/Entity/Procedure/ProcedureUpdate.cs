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
    }
}
