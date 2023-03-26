using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class RoadSegment : Entity
{
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(SocietyDomain);
    public override Type GetRepoEntityType() => RepoEntityType();
    private static Type RepoEntityType() => typeof(RoadSegment);

    public EntityRef<MapPolygonEdge> Edge { get; protected set; }
    public ModelRef<RoadModel> Road { get; protected set; }
    [SerializationConstructor] private RoadSegment(int id, EntityRef<MapPolygonEdge> edge,
        ModelRef<RoadModel> road) : base(id)
    {
        Edge = edge;
        Road = road;
    }
    
    public static RoadSegment Create(MapPolygonEdge edge, RoadModel road, CreateWriteKey key)
    {
        var rs =  new RoadSegment(key.IdDispenser.GetID(), edge.MakeRef(), road.MakeRef());
        key.Create(rs);
        return rs;
    }
}