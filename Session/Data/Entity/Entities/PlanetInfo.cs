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

    private PlanetInfo(object[] args, ServerWriteKey key) : base(args, key) { }

    private static PlanetInfo DeserializeConstructor(object[] args, ServerWriteKey key)
    {
        return new PlanetInfo(args, key);
    }
}