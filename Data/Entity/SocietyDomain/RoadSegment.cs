using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class RoadSegment : Entity
{
    public override Type GetDomainType() => typeof(SocietyDomain);

    public EntityRef<MapPolygonBorder> Border { get; private set; }

    private RoadSegment(int id, EntityRef<MapPolygonBorder> border) : base(id)
    {
        Border = border;
    }
    
    public static RoadSegment Create(int id, MapPolygonBorder border, CreateWriteKey key)
    {
        var borderRef = new EntityRef<MapPolygonBorder>(border, key);
        return new RoadSegment(id, borderRef);
    }
}