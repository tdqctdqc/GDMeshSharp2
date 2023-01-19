using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class PolygonBorder 
{
    public List<LineSegment> LowSegsRel { get; private set; }
    public List<LineSegment> HighSegsRel { get; private set; }
    public Polygon LowId { get; private set; }
    public Polygon HighId { get; private set; }
    public PolygonBorder(Polygon poly1, Polygon poly2, List<LineSegment> segments)
    {
        if (poly1.Id < poly2.Id)
        {
            LowId = poly1;
            HighId = poly2;
        }
        else
        {
            LowId = poly2;
            HighId = poly1;
        }

        HighSegsRel = OrderAndRelativizeSegments(segments, HighId);
        LowSegsRel = OrderAndRelativizeSegments(segments, LowId);
    }
    
    public PolygonBorder(Polygon poly1, List<LineSegment> poly1SegsRel, 
        Polygon poly2, List<LineSegment> poly2SegsRel)
    {
        List<LineSegment> abs1 = poly1SegsRel.Select(p => p.ChangeOrigin(poly1.Center, Vector2.Zero)).ToList();
        List<LineSegment> abs2 = poly2SegsRel.Select(p => p.ChangeOrigin(poly2.Center, Vector2.Zero)).ToList();
        if (poly1.Id < poly2.Id)
        {
            LowId = poly1;
            HighId = poly2;
            HighSegsRel = OrderAndRelativizeSegments(abs2, HighId);
            LowSegsRel = OrderAndRelativizeSegments(abs1, LowId);
        }
        else
        {
            LowId = poly2;
            HighId = poly1;
            HighSegsRel = OrderAndRelativizeSegments(abs1, HighId);
            LowSegsRel = OrderAndRelativizeSegments(abs2, LowId);
        }
    }
    private List<LineSegment> OrderAndRelativizeSegments(List<LineSegment> abs, Polygon poly)
    {
        var res = new List<LineSegment>();
        
        var first = abs[0];
        var leg1 = first.From - poly.Center;
        var leg2 = first.To - poly.Center;
        var leg1Angle = leg1.GetClockwiseAngleTo(leg2).RadToDegrees();
        var leg2Angle = leg2.GetClockwiseAngleTo(leg1).RadToDegrees();
        bool alter = leg1Angle < leg2Angle;
        
        for (var i = 0; i < abs.Count; i++)
        {
            var seg = abs[i];
            var t = seg.To - poly.Center;
            var f = seg.From - poly.Center;
            res.Add(alter ?  new LineSegment(t, f) : new LineSegment(f, t));
        }
        return res;
    }
    public void ReplacePoints(List<LineSegment> newSegmentsHiRel, List<LineSegment> newSegmentsLowRel)
    {
        HighSegsRel = newSegmentsHiRel;
        LowSegsRel = newSegmentsLowRel;
        // HighSegsRel.ForEach(s => s.Clamp(Root.Bounds.x));
        // LowSegsRel.ForEach(s => s.Clamp(Root.Bounds.x));
    }
}

public static class PolyBorderExt
{
    
    public static List<Vector2> GetPointsRel(this PolygonBorder b, Polygon p)
    {
        if (p == b.LowId) return b.LowSegsRel.GetPoints().ToList();
        if (p == b.HighId) return b.HighSegsRel.GetPoints().ToList();
        throw new Exception();
    }

    public static List<LineSegment> GetSegsRel(this PolygonBorder b, Polygon p)
    {
        if (p == b.LowId) return b.LowSegsRel;
        if (p == b.HighId) return b.HighSegsRel;
        throw new Exception();
    }
    public static List<LineSegment> GetSegsAbs(this PolygonBorder b)
    {
        return b.HighSegsRel.Select(s => s.ChangeOrigin(b.HighId.Center, Vector2.Zero)).ToList();
    }
    public static Vector2 GetOffsetToOtherPoly(this PolygonBorder b, Polygon p)
    {
        var other = b.GetOtherPoly(p);
        var otherCount = b.GetSegsRel(other).Count;
        return b.GetSegsRel(p)[0].From - b.GetSegsRel(other)[otherCount - 1].To;
    }
    public static Polygon GetOtherPoly(this PolygonBorder b, Polygon p)
    {
        if (p == b.LowId) return b.HighId;
        if (p == b.HighId) return b.LowId;
        throw new Exception();
    }
    public static List<Vector2> GetPointsAbs(this PolygonBorder b)
    {
        return b.HighSegsRel.GetPoints().Select(p => p + b.HighId.Center).ToList();
    }
}
