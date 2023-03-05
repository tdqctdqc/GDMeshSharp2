using Godot;
using System;
using MessagePack;

public class Player : Entity
{
    public override Type GetDomainType() => typeof(SocietyDomain);
    public Guid PlayerGuid { get; private set; }
    public string Name { get; private set; }
    public EntityRef<Regime> Regime { get; private set; }
    public static Player Create(int id, Guid userId, string name, CreateWriteKey key)
    {
        var p = new Player(id, userId, name, new EntityRef<Regime>(-1));
        key.Create(p);
        return p;
    }

    [SerializationConstructor] private Player(int id, Guid playerGuid, string name, EntityRef<Regime> regime) : base(id)
    {
        PlayerGuid = playerGuid;
        Name = name;
    }
    
}
