using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public sealed class Settlement : Location
{
    public EntityRef<MapPolygon> Poly { get; private set; }
    public float Size { get; private set; }

    public Settlement(int id, CreateWriteKey key, MapPolygon poly, float size) : base(id, key)
    {
        Poly = new EntityRef<MapPolygon>(poly, key) ;
        Size = size;
    }
    
    
    private static Settlement DeserializeConstructor(object[] args, ServerWriteKey key)
    {
        return new Settlement(args, key);
    }
    private Settlement(object[] args, ServerWriteKey key) : base(args, key) {}
}