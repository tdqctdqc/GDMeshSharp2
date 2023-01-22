using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public sealed class Settlement : Location
{
    public MapPolygon Poly { get; private set; }
    public float Size { get; private set; }

    public Settlement(int id, CreateWriteKey key, MapPolygon poly, float size) : base(id, key)
    {
        Poly = poly;
        Size = size;
    }
    
    
    private static Settlement DeserializeConstructor(string json)
    {
        return new Settlement(json);
    }
    private Settlement(string json) : base(json) { }
}