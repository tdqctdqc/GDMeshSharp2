using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class MapPolygonEdgeExt
{
    public static List<LineSegment> GetSegsAbs(this MapPolygonEdge b)
    {
        return b.HighSegsRel().Segments
            .Select(s => s.Translate(b.HighId.Entity().Center))
            .ToList().OrderEndToStart<LineSegment, Vector2>();
    }
    public static MapPolygon GetOtherPoly(this MapPolygonEdge b, MapPolygon p)
    {
        if (p == b.LowId.Entity()) return b.HighId.Entity();
        if (p == b.HighId.Entity()) return b.LowId.Entity();
        throw new Exception();
    }
    public static bool IsRegimeBorder(this MapPolygonEdge b)
    {
        return b.HighId.Entity().Regime.RefId != b.LowId.Entity().Regime.RefId;
    }
}