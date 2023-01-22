using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public sealed class PlanetInfo : Entity
{
    public Vector2 Dimensions { get; private set; }
    public PlanetInfo(Vector2 dimensions, int id, CreateWriteKey key) : base(id, key)
    {
        Dimensions = dimensions;
    }

    private PlanetInfo(string json) : base(json) { }

    private static PlanetInfo DeserializeConstructor(string json)
    {
        return new PlanetInfo(json);
    }
}