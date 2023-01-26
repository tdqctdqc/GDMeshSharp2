using Godot;
using System;

public abstract class Command
{
    public IMeta<Command> GetMeta() => Game.I.Serializer.GetCommandMeta(GetType());
    protected Command(WriteKey key)
    {
        
    }
    protected Command(object[] args)
    {
        GetMeta().Initialize(this, args);
    }

    public abstract void Enact(HostWriteKey key);
}
