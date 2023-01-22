using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public sealed class RoadSegment : Entity
{
    public EntityRef<MapPolygonBorder> Border { get; private set; }

    public RoadSegment(int id, CreateWriteKey key, MapPolygonBorder border) : base(id, key)
    {
        Border = EntityRef<MapPolygonBorder>.Construct(border, key);
    }

    public RoadSegment(string json) : base(json) { }
    private static RoadSegment DeserializeConstructor(string json)
    {
        return new RoadSegment(json);
    }
}