using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class MapPolygonBorder : Entity
{
    public float MoistureFlow { get; private set; }
    public List<LineSegment> LowSegsRel { get; private set; }
    public List<LineSegment> HighSegsRel { get; private set; }
    public EntityRef<MapPolygon> LowId { get; private set; }
    public EntityRef<MapPolygon> HighId { get; private set; }
    public MapPolygonBorder(int id, MapPolygon poly1, MapPolygon poly2, List<LineSegment> segments, CreateWriteKey key)
     : base(id, key)
    {
        if (poly1.Id < poly2.Id)
        {
            LowId = EntityRef<MapPolygon>.Construct(poly1, key);
            HighId = EntityRef<MapPolygon>.Construct(poly2, key);
        }
        else
        {
            LowId = EntityRef<MapPolygon>.Construct(poly2, key);
            HighId = EntityRef<MapPolygon>.Construct(poly1, key);
        }

        HighSegsRel = OrderAndRelativizeSegments(segments, HighId.Ref());
        LowSegsRel = OrderAndRelativizeSegments(segments, LowId.Ref());
    }

    public static MapPolygonBorder ConstructEdgeCase(int id, MapPolygon poly1, List<LineSegment> poly1SegsRel, 
        MapPolygon poly2, List<LineSegment> poly2SegsRel, GenWriteKey key)
    {
        var b = new MapPolygonBorder(id, poly1, poly1SegsRel, poly2, poly2SegsRel, key);
        key.Data.AddEntity(b, typeof(PlanetDomain), key);
        return b;
    }
    
    
    private MapPolygonBorder(int id, MapPolygon poly1, List<LineSegment> poly1SegsRel, 
        MapPolygon poly2, List<LineSegment> poly2SegsRel, GenWriteKey key) : base(id, key)
    {
        List<LineSegment> abs1 = poly1SegsRel.Select(p => p.ChangeOrigin(poly1.Center, Vector2.Zero)).ToList();
        List<LineSegment> abs2 = poly2SegsRel.Select(p => p.ChangeOrigin(poly2.Center, Vector2.Zero)).ToList();
        if (poly1.Id < poly2.Id)
        {
            LowId = EntityRef<MapPolygon>.Construct(poly1, key);
            HighId = EntityRef<MapPolygon>.Construct(poly2, key);
            HighSegsRel = OrderAndRelativizeSegments(abs2, HighId.Ref());
            LowSegsRel = OrderAndRelativizeSegments(abs1, LowId.Ref());
        }
        else
        {
            LowId = EntityRef<MapPolygon>.Construct(poly2, key);
            HighId = EntityRef<MapPolygon>.Construct(poly1, key);
            HighSegsRel = OrderAndRelativizeSegments(abs1, HighId.Ref());
            LowSegsRel = OrderAndRelativizeSegments(abs2, LowId.Ref());
        }
    }
    private List<LineSegment> OrderAndRelativizeSegments(List<LineSegment> abs, MapPolygon poly)
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
    public void ReplacePoints(List<LineSegment> newSegmentsHiRel, 
        List<LineSegment> newSegmentsLowRel, GenWriteKey key)
    {
        HighSegsRel = newSegmentsHiRel;
        LowSegsRel = newSegmentsLowRel;
        // HighSegsRel.ForEach(s => s.Clamp(Root.Bounds.x));
        // LowSegsRel.ForEach(s => s.Clamp(Root.Bounds.x));
    }
    
    public void SetFlow(float width, GenWriteKey key)
    {
        MoistureFlow = width;
    }
    public void IncrementFlow(float increment, GenWriteKey key)
    {
        MoistureFlow += increment;
    }
    
    private static MapPolygonBorder DeserializeConstructor(string json)
    {
        return new MapPolygonBorder(json);
    }
    private MapPolygonBorder(string json) : base(json) { }
}

public static class PolyBorderExt
{
    
    public static List<Vector2> GetPointsRel(this MapPolygonBorder b, MapPolygon p)
    {
        if (p == b.LowId.Ref()) return b.LowSegsRel.GetPoints().ToList();
        if (p == b.HighId.Ref()) return b.HighSegsRel.GetPoints().ToList();
        throw new Exception();
    }

    public static List<LineSegment> GetSegsRel(this MapPolygonBorder b, MapPolygon p)
    {
        if (p == b.LowId.Ref()) return b.LowSegsRel;
        if (p == b.HighId.Ref()) return b.HighSegsRel;
        throw new Exception();
    }
    public static List<LineSegment> GetSegsAbs(this MapPolygonBorder b)
    {
        return b.HighSegsRel.Select(s => s.ChangeOrigin(b.HighId.Ref().Center, Vector2.Zero)).ToList();
    }
    public static Vector2 GetOffsetToOtherPoly(this MapPolygonBorder b, MapPolygon p)
    {
        var other = b.GetOtherPoly(p);
        var otherCount = b.GetSegsRel(other).Count;
        return b.GetSegsRel(p)[0].From - b.GetSegsRel(other)[otherCount - 1].To;
    }
    public static MapPolygon GetOtherPoly(this MapPolygonBorder b, MapPolygon p)
    {
        if (p == b.LowId.Ref()) return b.HighId.Ref();
        if (p == b.HighId.Ref()) return b.LowId.Ref();
        throw new Exception();
    }
    public static List<Vector2> GetPointsAbs(this MapPolygonBorder b)
    {
        return b.HighSegsRel.GetPoints().Select(p => p + b.HighId.Ref().Center).ToList();
    }

    
}
