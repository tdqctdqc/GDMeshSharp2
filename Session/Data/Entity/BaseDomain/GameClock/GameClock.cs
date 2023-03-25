
using System;
using MessagePack;

public class GameClock : Entity
{
    public override Type GetRepoEntityType() => RepoEntityType();
    private static Type RepoEntityType() => typeof(GameClock);
    public int Tick { get; private set; }
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

    public override Type GetDomainType() => typeof(BaseDomain);
}
