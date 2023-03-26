
using System;
using MessagePack;

public class GameClock : Entity
{
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(BaseDomain);
    public int Tick { get; protected set; }
    public static GameClock Create(GenWriteKey key)
    {
        var gc = new GameClock(key.IdDispenser.GetID(), 0);
        key.Create(gc);
        return gc;
    }
    [SerializationConstructor] private GameClock(int id, int tick) : base(id)
    {
        Tick = tick;
    }

}
