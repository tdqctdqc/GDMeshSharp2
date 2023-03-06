using Godot;
using System;
using MessagePack;

public abstract class Command
{
    [IgnoreMember] public Guid CommandingPlayerGuid { get; private set; }
    protected Command()
    {

    }
    public abstract void Enact(HostWriteKey key);
    public abstract bool Valid(Data data);

    public void SetGuid(Guid guid)
    {
        CommandingPlayerGuid = guid;
    }
}
