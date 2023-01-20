using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public sealed class RoadSegment : Entity
{
    public EntityRef<GenPolygon> P1 { get; private set; }
    public EntityRef<GenPolygon> P2 { get; private set; }

    public RoadSegment(int id, CreateWriteKey key, GenPolygon p1, GenPolygon p2) : base(id, key)
    {
        P1 = p1.Id > p2.Id ? new EntityRef<GenPolygon>(p1)  : new EntityRef<GenPolygon>(p2);
        P2 = P1.RefId == p1.Id ? new EntityRef<GenPolygon>(p2)  : new EntityRef<GenPolygon>(p1);
    }

    public RoadSegment(int id, CreateWriteKey key) : base(id, key)
    {
    }

    public RoadSegment(string json) : base(json) { }
    private static RoadSegment DeserializeConstructor(string json)
    {
        return new RoadSegment(json);
    }
}