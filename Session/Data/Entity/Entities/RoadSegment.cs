using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public sealed class RoadSegment : Entity
{
    public EntityRef<MapPolygonBorder> Border { get; private set; }

    public RoadSegment(int id, CreateWriteKey key, MapPolygonBorder border) : base(id, key)
    {
        Border = new EntityRef<MapPolygonBorder>(border, key);
    }

    public RoadSegment(object[] args) : base(args) { }
    private static RoadSegment DeserializeConstructor(object[] args)
    {
        return new RoadSegment(args);
    }
}