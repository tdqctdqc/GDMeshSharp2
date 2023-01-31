using Godot;
using System;

public abstract class Procedure
{
    protected Procedure(HostWriteKey key)
    {
        
    }
    public abstract void Enact(ProcedureWriteKey key);
}

