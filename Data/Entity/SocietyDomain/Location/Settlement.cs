using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;


public class Settlement : Location
{
    public override Type GetDomainType() => typeof(SocietyDomain);
    public EntityRef<MapPolygon> Poly { get; private set; }
    public float Size { get; private set; }
    public string Name { get; private set; }
    
    public static Settlement Create(int id, string name, MapPolygon poly, float size, CreateWriteKey key)
    {
        return new Settlement(id, new EntityRef<MapPolygon>(poly.Id), size, name);
    }
    [SerializationConstructor] private Settlement(int id, EntityRef<MapPolygon> poly, float size,
        string name) : base(id)
    {
        Name = name;
        Poly = poly;
        Size = size;
    }

}