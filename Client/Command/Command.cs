using Godot;
using System;

public abstract class Command
{
    public Guid PlayerGuid { get; private set; }
    protected Command(WriteKey key)
    {
        PlayerGuid = Game.I.PlayerGuid;
    }
    public abstract void Enact(HostWriteKey key);
    public abstract bool Valid(Data data);
}
