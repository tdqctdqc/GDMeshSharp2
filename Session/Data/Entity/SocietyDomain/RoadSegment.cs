using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class RoadSegment : Entity
{
    public override Type GetDomainType() => typeof(SocietyDomain);

    public EntityRef<MapPolygonEdge> Edge { get; private set; }

    [SerializationConstructor] private RoadSegment(int id, EntityRef<MapPolygonEdge> edge) : base(id)
    {
        Edge = edge;
    }
    
    public static RoadSegment Create(MapPolygonEdge edge, CreateWriteKey key)
    {
        var rs =  new RoadSegment(key.IdDispenser.GetID(), edge.MakeRef());
        key.Create(rs);
        return rs;
    }
}