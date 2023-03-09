using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class MapPolygonEdgeExt
{
    
    public static List<Vector2> GetPointsRel(this MapPolygonEdge b, MapPolygon p)
    {
        if (p == b.LowId.Entity()) return b.LowSegsRel().Segments.GetPoints().ToList();
        if (p == b.HighId.Entity()) return b.HighSegsRel().Segments.GetPoints().ToList();
        throw new Exception();
    }
    public static List<LineSegment> GetSegsRel(this MapPolygonEdge b, MapPolygon p)
    {
        if (p == b.LowId.Entity()) return b.LowSegsRel().Segments;
        if (p == b.HighId.Entity()) return b.HighSegsRel().Segments;
        throw new Exception();
    }
    public static PolyBorderChain GetBorder(this MapPolygonEdge b, MapPolygon p)
    {
        if (p == b.LowId.Entity()) return b.LowSegsRel();
        if (p == b.HighId.Entity()) return b.HighSegsRel();
        throw new Exception();
    }
    public static List<LineSegment> GetSegsAbs(this MapPolygonEdge b)
    {
        return b.HighSegsRel().Segments
            .Select(s => s.Translate(b.HighId.Entity().Center))
            .ToList().OrderEndToStart();
    }
    public static Vector2 GetOffsetToOtherPoly(this MapPolygonEdge b, MapPolygon p)
    {
        var other = b.GetOtherPoly(p);
        var otherCount = b.GetSegsRel(other).Count;
        return b.GetSegsRel(p)[0].From - b.GetSegsRel(other)[otherCount - 1].To;
    }
    public static MapPolygon GetOtherPoly(this MapPolygonEdge b, MapPolygon p)
    {
        if (p == b.LowId.Entity()) return b.HighId.Entity();
        if (p == b.HighId.Entity()) return b.LowId.Entity();
        throw new Exception();
    }
    public static List<Vector2> GetPointsAbs(this MapPolygonEdge b)
    {
        return b.HighSegsRel().Segments.GetPoints().Select(p => p + b.HighId.Entity().Center).ToList();
    }
    public static bool IsRegimeBorder(this MapPolygonEdge b)
    {
        return b.HighId.Entity().Regime.RefId != b.LowId.Entity().Regime.RefId;
    }
}