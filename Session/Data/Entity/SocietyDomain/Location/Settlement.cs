using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;


public class Settlement : Location
{
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(SocietyDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public EntityRef<MapPolygon> Poly { get; protected set; }
    public int Size { get; protected set; }
    public string Name { get; protected set; }
    
    public static Settlement Create(string name, MapPolygon poly, int size, CreateWriteKey key)
    {
        var s = new Settlement(key.IdDispenser.GetID(), poly.MakeRef(), size, name);
        key.Create(s);
        return s;
    }
    [SerializationConstructor] private Settlement(int id, EntityRef<MapPolygon> poly, int size,
        string name) : base(id)
    {
        Name = name;
        Poly = poly;
        Size = size;
    }

}