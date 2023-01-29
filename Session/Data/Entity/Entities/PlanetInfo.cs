using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Godot;

public sealed class PlanetInfo : Entity
{
    public Vector2 Dimensions { get; private set; }
    public PlanetInfo(Vector2 dimensions, int id, CreateWriteKey key) : base(id, key)
    {
        Dimensions = dimensions;
    }

    [JsonConstructor] public PlanetInfo(int id, Vector2 dimensions) : base(id)
    {
        Dimensions = dimensions;
    }
}