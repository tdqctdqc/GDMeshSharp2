using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class RoadSegment : Entity
{
    public override Type GetDomainType() => typeof(SocietyDomain);

    public EntityRef<MapPolygonBorder> Border { get; private set; }

    [SerializationConstructor] private RoadSegment(int id, EntityRef<MapPolygonBorder> border) : base(id)
    {
        Border = border;
    }
    
    public static RoadSegment Create(int id, MapPolygonBorder border, CreateWriteKey key)
    {
        var rs =  new RoadSegment(id, border.MakeRef());
        key.Create(rs);
        return rs;
    }
}