using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;


public class Peep : Entity
{
    public override Type GetDomainType() => typeof(SocietyDomain);
    public EntityRef<MapPolygon> Home { get; private set; }
    public ModelRef<PeepJob> Job { get; private set; }

    public static Peep Create(EntityRef<MapPolygon> home,
        ModelRef<PeepJob> job, CreateWriteKey key)
    {
        var p = new Peep(key.IdDispenser.GetID(), home, job);
        key.Create(p);
        return p;
    }
    [SerializationConstructor] private Peep(int id, EntityRef<MapPolygon> home, 
        ModelRef<PeepJob> job) : base(id)
    {
        Home = home;
        Job = job;
    }
}