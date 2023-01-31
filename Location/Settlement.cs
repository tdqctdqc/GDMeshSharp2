using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;


public class Settlement : Location
{
    public EntityRef<MapPolygon> Poly { get; private set; }
    public float Size { get; private set; }

    public Settlement(int id, EntityRef<MapPolygon> poly, float size) : base(id)
    {
        Poly = poly;
        Size = size;
    }

    public Settlement(int id, CreateWriteKey key, MapPolygon poly, float size) : base(id, key)
    {
        Poly = new EntityRef<MapPolygon>(poly, key) ;
        Size = size;
    }
}