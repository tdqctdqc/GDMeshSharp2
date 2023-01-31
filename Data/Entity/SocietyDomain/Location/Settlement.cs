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
    
    public static Settlement Create(int id, MapPolygon poly, float size, CreateWriteKey key)
    {
        return new Settlement(id, new EntityRef<MapPolygon>(poly.Id), size);
    }
    public Settlement(int id, EntityRef<MapPolygon> poly, float size) : base(id)
    {
        Poly = poly;
        Size = size;
    }

}