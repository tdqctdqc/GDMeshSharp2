using Godot;
using System;

public abstract class Update
{
    protected Update(HostWriteKey key)
    {
        
    }

    public abstract void Enact(ServerWriteKey key);
}
