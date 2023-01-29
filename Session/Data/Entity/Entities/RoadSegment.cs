using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Godot;

public sealed class RoadSegment : Entity
{
    public EntityRef<MapPolygonBorder> Border { get; private set; }


    [JsonConstructor] public RoadSegment(int id, EntityRef<MapPolygonBorder> border) : base(id)
    {
        Border = border;
    }

    public RoadSegment(int id, CreateWriteKey key, MapPolygonBorder border) : base(id, key)
    {
        Border = new EntityRef<MapPolygonBorder>(border, key);
    }
}