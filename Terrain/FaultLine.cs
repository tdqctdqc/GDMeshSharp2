using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FaultLine 
{
    public List<List<LineSegment>> Segments { get; private set; }
    public GenPlate LowId { get; private set; }
    public GenPlate HighId { get; private set; }
    public List<MapPolygon> PolyFootprint { get; private set; }
    public float Friction { get; private set; }
    public MapPolygon Origin => HighId.GetSeedPoly();
    public List<BorderEdge<MapPolygon>> Edges { get; private set; }
    public FaultLine(float friction, GenPlate highId, 
        GenPlate lowId, List<BorderEdge<MapPolygon>> edgesHi,
        WorldData data)
    {
        Friction = friction;
        HighId = highId;
        LowId = lowId;
        PolyFootprint = new List<MapPolygon>();
        Segments = edgesHi.Select(
            e => e.Native.GetBorder(e.Foreign, data)
                .GetSegsRel(e.Native)
                .Select(l => l.ChangeOrigin(e.Native.Center, Origin.Center))
                .ToList())
            .ToList();
        Segments.ForEach(ss => ss.ForEach(s => s.Clamp(data.Planet.Width)));
        Edges = edgesHi;
    }

    public float GetDist(MapPolygon poly, WorldData data)
    {
        return Segments.Select(seg => seg.Select(l => l.DistanceTo(Origin.GetOffsetTo(poly, data))).Min()).Min();
    }
    public bool PointWithinDist(Vector2 pointAbs, float dist, WorldData data)
    {
        return Segments.Any(seg => seg.Any(l => l.DistanceTo(Origin.GetOffsetTo(pointAbs, data)) < dist));
    }
}