using Godot;
using System;
using MessagePack;

public class Player : Entity
{
    public override Type GetDomainType() => typeof(SocietyDomain);
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public static Player Create(int id, Guid userId, string name, CreateWriteKey key)
    {
        return new Player(id, userId, name);
    }

    private Player(int id, Guid userId, string name) : base(id)
    {
        UserId = userId;
        Name = name;
    }

}
