using Godot;
using System;

public abstract class Update
{
    public IMeta<Update> GetMeta() => Game.I.Serializer.GetUpdateMeta(GetType());
    protected Update(HostWriteKey key)
    {
        
    }
    protected Update(object[] args)
    {
        GetMeta().Initialize(this, args);
    }

    public abstract void Enact(ServerWriteKey key);
}
