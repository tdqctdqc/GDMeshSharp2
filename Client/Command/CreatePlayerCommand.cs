
using System;
using Godot;

public class CreatePlayerCommand : Command
{
    public Guid Guid { get; private set; }
    public string Name { get; private set; }

    public CreatePlayerCommand(WriteKey key, Guid guid, string name) : base(key)
    {
        Guid = guid;
        Name = name;
    }

    public override void Enact(HostWriteKey key)
    {
        Player.Create(key.IdDispenser.GetID(), Guid, Name, key);
    }
    public override bool Valid(Data data)
    {
        return data.BaseDomain.Players.ByGuid.ContainsKey(Guid) == false;
    }
}
