using Godot;
using System;

public abstract class Command
{
    protected Command(WriteKey key)
    {
        
    }
    public abstract void Enact(HostWriteKey key);
    public abstract bool Valid(Data data);
}
