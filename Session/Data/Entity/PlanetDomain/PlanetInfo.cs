using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class PlanetInfo : Entity
{
    public override Type GetRepoEntityType() => RepoEntityType();
    private static Type RepoEntityType() => typeof(PlanetInfo);
    public override Type GetDomainType() => typeof(PlanetDomain);
    public Vector2 Dimensions { get; protected set; }
    public static PlanetInfo Create(Vector2 dimensions, CreateWriteKey key)
    {
        var pi =  new PlanetInfo(key.IdDispenser.GetID(), dimensions);
        key.Create(pi);
        return pi;
    }
    
    [SerializationConstructor] private PlanetInfo(int id, Vector2 dimensions) : base(id)
    {
        Dimensions = dimensions;
    }
}