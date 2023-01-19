using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FaultLine 
{
    public List<List<LineSegment>> Segments { get; private set; }
    public GenPlate LowId { get; private set; }
    public GenPlate HighId { get; private set; }
    public List<GenPolygon> PolyFootprint { get; private set; }
    public float Friction { get; private set; }
    public GenPolygon Origin => HighId.GetSeedPoly();
    public List<BorderEdge<GenPolygon>> Edges { get; private set; }
    public FaultLine(float friction, GenPlate highId, 
        GenPlate lowId, List<BorderEdge<GenPolygon>> edgesHi,
        WorldData data)
    {
        Friction = friction;
        HighId = highId;
        LowId = lowId;
        PolyFootprint = new List<GenPolygon>();
        Segments = edgesHi.Select(
            e => e.Native.GetPolyBorder(e.Foreign)
                .GetSegsRel(e.Native)
                .Select(l => l.ChangeOrigin(e.Native.Center, Origin.Center))
                .ToList())
            .ToList();
        Segments.ForEach(ss => ss.ForEach(s => s.Clamp(data.Dimensions.x)));
        Edges = edgesHi;
    }

    public float GetDist(Polygon poly, WorldData data)
    {
        return Segments.Select(seg => seg.Select(l => l.DistanceTo(Origin.GetOffsetTo(poly, data.Dimensions.x))).Min()).Min();
    }
    public bool PointWithinDist(Vector2 pointAbs, float dist, WorldData data)
    {
        return Segments.Any(seg => seg.Any(l => l.DistanceTo(Origin.GetOffsetTo(pointAbs, data.Dimensions.x)) < dist));
    }
}