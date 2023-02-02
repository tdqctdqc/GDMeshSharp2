using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;


public class MapPolygonBorder : Entity
{
    public override Type GetDomainType() => typeof(PlanetDomain);
    public float MoistureFlow { get; private set; }
    public List<LineSegment> LowSegsRel { get; private set; }
    public List<LineSegment> HighSegsRel { get; private set; }
    public EntityRef<MapPolygon> LowId { get; private set; }
    public EntityRef<MapPolygon> HighId { get; private set; }

    [SerializationConstructor] private MapPolygonBorder(int id, float moistureFlow, List<LineSegment> lowSegsRel, 
        List<LineSegment> highSegsRel, EntityRef<MapPolygon> lowId, 
        EntityRef<MapPolygon> highId) : base(id)
    {
        MoistureFlow = moistureFlow;
        LowSegsRel = lowSegsRel;
        HighSegsRel = highSegsRel;
        LowId = lowId;
        HighId = highId;
    }

    public static MapPolygonBorder Create(int id, MapPolygon poly1, MapPolygon poly2, 
        List<LineSegment> segments, GenWriteKey key)
    {
        EntityRef<MapPolygon> lowId;
        EntityRef<MapPolygon> highId;
        
        if (poly1.Id < poly2.Id)
        {
            lowId = new EntityRef<MapPolygon>(poly1, key);
            highId = new EntityRef<MapPolygon>(poly2, key);
        }
        else
        {
            lowId = new EntityRef<MapPolygon>(poly2, key);
            highId = new EntityRef<MapPolygon>(poly1, key);
        }
        
        var highSegsRel = OrderAndRelativizeSegments(segments, highId.Ref(), key.Data);
        var lowSegsRel = OrderAndRelativizeSegments(segments, lowId.Ref(), key.Data);
        
        var b =  new MapPolygonBorder(
            id, 0f, lowSegsRel, highSegsRel, lowId, highId);
        poly1.AddNeighbor(poly2, b, key);
        poly2.AddNeighbor(poly1, b, key);
        key.Create(b);
        Test(b, key);
        return b;
    }

    public static MapPolygonBorder CreateEdgeCase(int id, MapPolygon poly1, List<LineSegment> poly1SegsRel, 
        MapPolygon poly2, List<LineSegment> poly2SegsRel, GenWriteKey key)
    {
        // var b = new MapPolygonBorder(id, poly1, poly1SegsRel, poly2, poly2SegsRel, key);
        List<LineSegment> abs1 = poly1SegsRel.Select(p => p.ChangeOrigin(poly1.Center, Vector2.Zero)).ToList();
        List<LineSegment> abs2 = poly2SegsRel.Select(p => p.ChangeOrigin(poly2.Center, Vector2.Zero)).ToList();

        EntityRef<MapPolygon> lowId;
        EntityRef<MapPolygon> highId;
        List<LineSegment> lowSegsRel;
        List<LineSegment> highSegsRel;
        if (poly1.Id < poly2.Id)
        {
            lowId = new EntityRef<MapPolygon>(poly1, key);
            highId = new EntityRef<MapPolygon>(poly2, key);
            highSegsRel = OrderAndRelativizeSegments(abs2, highId.Ref(), key.Data);
            lowSegsRel = OrderAndRelativizeSegments(abs1, lowId.Ref(), key.Data);
        }
        else
        {
            lowId = new EntityRef<MapPolygon>(poly2, key);
            highId = new EntityRef<MapPolygon>(poly1, key);
            highSegsRel = OrderAndRelativizeSegments(abs1, highId.Ref(), key.Data);
            lowSegsRel = OrderAndRelativizeSegments(abs2, lowId.Ref(), key.Data);
        }

        var b =  new MapPolygonBorder(
            id, 0f, lowSegsRel, highSegsRel, lowId, highId);
        poly1.AddNeighbor(poly2, b, key);
        poly2.AddNeighbor(poly1, b, key);
        key.Create(b);
        Test(b, key);
        return b;
    }

    private static void Test(MapPolygonBorder b, WriteKey key)
    {
        var border = b.HighId.Ref().GetBorder(b.LowId.Ref(), key.Data);
        if (border == null) throw new Exception();
    }
    private static List<LineSegment> OrderAndRelativizeSegments(List<LineSegment> abs, MapPolygon poly, Data data)
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
            var t = poly.GetOffsetTo(seg.To, data);
            var f = poly.GetOffsetTo(seg.From, data);
            res.Add(alter ?  new LineSegment(t, f) : new LineSegment(f, t));
        }
        return res;
    }
    public void ReplacePoints(List<LineSegment> newSegmentsHiRel, 
        List<LineSegment> newSegmentsLowRel, GenWriteKey key)
    {
        HighSegsRel = newSegmentsHiRel;
        LowSegsRel = newSegmentsLowRel;
    }
    
    public void SetFlow(float width, GenWriteKey key)
    {
        MoistureFlow = width;
    }
    public void IncrementFlow(float increment, GenWriteKey key)
    {
        MoistureFlow += increment;
    }
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
