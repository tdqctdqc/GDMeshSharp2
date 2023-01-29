using Godot;
using System;
using System.Text.Json;

[EntityProcedure]
public abstract class Procedure
{
    protected Procedure(HostWriteKey key)
    {
        
    }
    public abstract void Enact(StrongWriteKey key);
}

