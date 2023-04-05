
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

    public override void Enact(HostWriteKey key, Action<Procedure> queueProc)
    {
        Player.Create(Guid, Name, key);
    }
    public override bool Valid(Data data)
    {
        return data.BaseDomain.PlayerAux.ByGuid.ContainsKey(Guid) == false;
    }
}
