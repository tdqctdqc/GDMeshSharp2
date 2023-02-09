using Godot;
using System;

public abstract class Update
{
    protected Update()
    {
        
    }

    public abstract void Enact(ServerWriteKey key);
}
