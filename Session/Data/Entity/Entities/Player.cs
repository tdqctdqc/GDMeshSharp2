using Godot;
using System;

public sealed class Player : Entity
{
    public EntityVar<Guid> UserId { get; private set; }
    public EntityVar<string> Name { get; private set; }
    public Player(int id, string name, HostWriteKey key) : base(id, key)
    {
        Name = EntityVar<string>.Construct(name, this, nameof(Name));
    }
    private static Player DeserializeConstructor(string json)
    {
        return new Player(json);
    }
    private Player(string json) : base(json) { }
}
