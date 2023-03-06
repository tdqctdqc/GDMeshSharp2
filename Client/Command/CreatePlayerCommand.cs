
using System;
using Godot;

public class CreatePlayerCommand : Command
{
    public Guid Guid { get; private set; }
    public string Name { get; private set; }

    public static CreatePlayerCommand Construct(Guid guid, string name)
    {
        return new CreatePlayerCommand(guid, name);
    }
    private CreatePlayerCommand(Guid guid, string name) : base()
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
