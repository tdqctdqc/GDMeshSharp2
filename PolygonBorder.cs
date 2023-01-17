using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class PolygonBorder 
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

        HighSegsRel = ReceiveSegmentsAbs(segments, HighId);
        LowSegsRel = ReceiveSegmentsAbs(segments, LowId);
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
            HighSegsRel = ReceiveSegmentsAbs(abs2, HighId);
            LowSegsRel = ReceiveSegmentsAbs(abs1, LowId);
        }
        else
        {
            LowId = poly2;
            HighId = poly1;
            HighSegsRel = ReceiveSegmentsAbs(abs1, HighId);
            LowSegsRel = ReceiveSegmentsAbs(abs2, LowId);
        }
    }
    private List<LineSegment> ReceiveSegmentsAbs(List<LineSegment> abs, Polygon poly)
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
    public List<Vector2> GetPointsRel(Polygon p)
    {
        if (p == LowId) return LowSegsRel.GetPoints().ToList();
        if (p == HighId) return HighSegsRel.GetPoints().ToList();
        throw new Exception();
    }

    public List<LineSegment> GetSegsRel(Polygon p)
    {
        if (p == LowId) return LowSegsRel;
        if (p == HighId) return HighSegsRel;
        throw new Exception();
    }
    public List<LineSegment> GetSegsAbs()
    {
        return HighSegsRel.Select(s => s.ChangeOrigin(HighId.Center, Vector2.Zero)).ToList();
    }
    public Vector2 GetOffsetToOtherPoly(Polygon p)
    {
        var other = GetOtherPoly(p);
        var otherCount = GetSegsRel(other).Count;
        return GetSegsRel(p)[0].From - GetSegsRel(other)[otherCount - 1].To;
    }
    public Polygon GetOtherPoly(Polygon p)
    {
        if (p == LowId) return HighId;
        if (p == HighId) return LowId;
        throw new Exception();
    }
    public List<Vector2> GetPointsAbs()
    {
        return HighSegsRel.GetPoints().Select(p => p + HighId.Center).ToList();
    }
    public void ReplacePoints(List<LineSegment> newSegmentsHiRel, List<LineSegment> newSegmentsLowRel)
    {
        HighSegsRel = newSegmentsHiRel;
        LowSegsRel = newSegmentsLowRel;
        // HighSegsRel.ForEach(s => s.Clamp(Root.Bounds.x));
        // LowSegsRel.ForEach(s => s.Clamp(Root.Bounds.x));
    }
}
