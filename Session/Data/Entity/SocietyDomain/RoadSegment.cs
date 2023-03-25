using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class RoadSegment : Entity
{
    public override Type GetDomainType() => typeof(SocietyDomain);
    public override Type GetRepoEntityType() => GetType();

    public EntityRef<MapPolygonEdge> Edge { get; private set; }
    public ModelRef<RoadModel> Road { get; private set; }
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