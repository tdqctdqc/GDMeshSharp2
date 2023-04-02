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
    public int Size { get; private set; }
    public static Peep Create(MapPolygon home, int size, CreateWriteKey key)
    {
        var id = key.IdDispenser.GetID();
        var p = new Peep(id, home.MakeRef(), size);
        key.Create(p);
        return p;
    }
    [SerializationConstructor] private Peep(int id, EntityRef<MapPolygon> home,
        int size) : base(id)
    {
        if (size <= 0) throw new Exception();
        Home = home;
        Size = size;
    }
    public void GrowSize(int delta, ProcedureWriteKey key)
    {
        if (delta <= 0) throw new Exception();
        Size += delta;
    }
}