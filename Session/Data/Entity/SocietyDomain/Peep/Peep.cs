using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;


public class Peep : Entity
{
    public override Type GetRepoEntityType() => RepoEntityType();
    private static Type RepoEntityType() => typeof(Peep);
    public override Type GetDomainType() => typeof(SocietyDomain);
    public EntityRef<MapPolygon> Home { get; protected set; }
    public ModelRef<PeepJob> Job { get; protected set; }

    public static Peep Create(MapPolygon home,
        PeepJob job, CreateWriteKey key)
    {
        var p = new Peep(key.IdDispenser.GetID(), home.MakeRef(), job.MakeRef());
        key.Create(p);
        return p;
    }
    [SerializationConstructor] protected Peep(int id, EntityRef<MapPolygon> home, 
        ModelRef<PeepJob> job) : base(id)
    {
        Home = home;
        Job = job;
    }
}