using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class PlanetInfo : Entity
{
    public Vector2 Dimensions { get; private set; }
    public PlanetInfo(Vector2 dimensions, int id, CreateWriteKey key) : base(id, key)
    {
        Dimensions = dimensions;
    }

    public PlanetInfo(int id, Vector2 dimensions) : base(id)
    {
        Dimensions = dimensions;
    }
}