using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FaultLine 
{
    public List<List<LineSegment>> Segments { get; private set; }
    public GeologyPlate LowId { get; private set; }
    public GeologyPlate HighId { get; private set; }
    public List<GeologyPolygon> PolyFootprint { get; private set; }
    public float Friction { get; private set; }
    public GeologyPolygon Origin => HighId.SeedPoly;
    public List<Edge<GeologyPolygon>> Edges { get; private set; }
    public FaultLine(float friction, GeologyPlate highId, GeologyPlate lowId, List<Edge<GeologyPolygon>> edgesHi)
    {
        Friction = friction;
        HighId = highId;
        LowId = lowId;
        PolyFootprint = new List<GeologyPolygon>();
        Segments = edgesHi.Select(
            e => e.Native.GetPolyBorder(e.Foreign)
                .GetSegsRel(e.Native)
                .Select(l => l.ChangeOrigin(e.Native.Center, Origin.Center))
                .ToList())
            .ToList();
        Segments.ForEach(ss => ss.ForEach(s => s.Clamp(Root.Bounds.x)));
        Edges = edgesHi;
    }

    public float GetDist(Polygon poly)
    {
        return Segments.Select(seg => seg.Select(l => l.DistanceTo(Origin.GetOffsetTo(poly, Root.Bounds.x))).Min()).Min();
    }
    public bool PointWithinDist(Vector2 pointAbs, float dist)
    {
        return Segments.Any(seg => seg.Any(l => l.DistanceTo(Origin.GetOffsetTo(pointAbs, Root.Bounds.x)) < dist));
    }
}