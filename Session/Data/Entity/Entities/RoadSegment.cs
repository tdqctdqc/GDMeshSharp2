using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class RoadSegment : Entity
{
    public EntityRef<MapPolygonBorder> Border { get; private set; }

    public RoadSegment(int id, EntityRef<MapPolygonBorder> border) : base(id)
    {
        Border = border;
    }

    public RoadSegment(int id, CreateWriteKey key, MapPolygonBorder border) : base(id, key)
    {
        Border = new EntityRef<MapPolygonBorder>(border, key);
    }
}