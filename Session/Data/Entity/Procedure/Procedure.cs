using Godot;
using System;
using System.Text.Json;

[EntityProcedure]
public abstract class Procedure
{
    public IMeta<Procedure> GetMeta() => Game.I.Serializer.GetProcedureMeta(GetType());
    protected Procedure(HostWriteKey key)
    {
        
    }
    protected Procedure(object[] args)
    {
        GetMeta().Initialize(this, args);
    }
    public abstract void Enact(StrongWriteKey key);
}

