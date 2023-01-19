using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public sealed class MapPolygon : Entity
{
    public MapPolygon(int id, CreateWriteKey key) : base(id, key)
    {
    }

    public MapPolygon(string json) : base(json)
    {
    }
}