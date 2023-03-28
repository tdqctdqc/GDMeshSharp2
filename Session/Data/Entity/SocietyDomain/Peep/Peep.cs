using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;


public class Peep : Entity
{
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(SocietyDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public EntityRef<MapPolygon> Home { get; protected set; }
    public ModelRef<PeepJob> Job { get; protected set; }
    public int Size { get; private set; }
    

    public static Peep Create(MapPolygon home,
        PeepJob job, int size, CreateWriteKey key)
    {
        var p = new Peep(key.IdDispenser.GetID(), home.MakeRef(), job.MakeRef(), size);
        key.Create(p);
        return p;
    }
    [SerializationConstructor] private Peep(int id, EntityRef<MapPolygon> home, 
        ModelRef<PeepJob> job, int size) : base(id)
    {
        Home = home;
        Job = job;
        Size = size;
    }
}