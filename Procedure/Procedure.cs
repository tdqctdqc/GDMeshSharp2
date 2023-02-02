using Godot;
using System;

public abstract class Procedure
{
    protected Procedure()
    {
        
    }

    public abstract bool Valid(Data data);
    public abstract void Enact(ProcedureWriteKey key);
}

