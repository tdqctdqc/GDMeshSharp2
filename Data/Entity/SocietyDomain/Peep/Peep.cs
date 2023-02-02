using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;


public class Peep : Entity
{
    public override Type GetDomainType() => typeof(SocietyDomain);

    public int Size { get; private set; }
    public EntityRef<MapPolygon> Home { get; private set; }
    public ModelRef<PeepJob> Job { get; private set; }

    public static Peep Create(int id, int size, EntityRef<MapPolygon> home,
        ModelRef<PeepJob> job, CreateWriteKey key)
    {
        var p = new Peep(id, size, home, job);
        key.Create(p);
        return p;
    }
    [SerializationConstructor] private Peep(int id, int size, EntityRef<MapPolygon> home, 
        ModelRef<PeepJob> job) : base(id)
    {
        Size = size;
        Home = home;
        Job = job;
    }
}