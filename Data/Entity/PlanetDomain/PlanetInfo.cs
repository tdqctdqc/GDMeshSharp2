using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class PlanetInfo : Entity
{
    public override Type GetDomainType() => typeof(PlanetDomain);
    public Vector2 Dimensions { get; private set; }
    public static PlanetInfo Create(Vector2 dimensions, int id, CreateWriteKey key)
    {
        return new PlanetInfo(id, dimensions);
    }
    
    private PlanetInfo(int id, Vector2 dimensions) : base(id)
    {
        Dimensions = dimensions;
    }
}