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
    private int _riverSegIndexHi = -1;
    [SerializationConstructor] private MapPolygonBorder(int id, float moistureFlow, List<LineSegment> lowSegsRel, 
        List<LineSegment> highSegsRel, EntityRef<MapPolygon> lowId, 
        EntityRef<MapPolygon> highId) : base(id)
    {
        MoistureFlow = moistureFlow;
        LowSegsRel = lowSegsRel;
        HighSegsRel = highSegsRel;
        if (lowSegsRel.Count != highSegsRel.Count) throw new Exception();
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
        
        var highSegsRel = OrderAndRelativizeSegments(segments, highId.Entity(), key.Data);
        var lowSegsRel = OrderAndRelativizeSegments(segments, lowId.Entity(), key.Data);
        
        var b =  new MapPolygonBorder(
            id, 0f, lowSegsRel, highSegsRel, lowId, highId);
        poly1.AddNeighbor(poly2, b, key);
        poly2.AddNeighbor(poly1, b, key);
        key.Create(b);
        return b;
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
            if (t.Length() > 1000f || f.Length() > 1000f) throw new Exception();
            res.Add(alter ?  new LineSegment(t, f) : new LineSegment(f, t));
        }

        if (res.IsContinuous() == false) res.Reverse();
        return res;
    }
    public void ReplacePoints(List<LineSegment> newSegmentsAbs, 
        GenWriteKey key)
    {
        HighSegsRel = OrderAndRelativizeSegments(newSegmentsAbs, HighId.Entity(), key.Data);
        LowSegsRel = OrderAndRelativizeSegments(newSegmentsAbs, LowId.Entity(), key.Data);
        if (HighSegsRel.Count != LowSegsRel.Count) throw new Exception();
    }
    
    public void SetFlow(float width, GenWriteKey key)
    {
        MoistureFlow = width;
    }
    public void IncrementFlow(float increment, GenWriteKey key)
    {
        MoistureFlow += increment;
    }
    public void SetRiverIndexHi(int i, GenWriteKey key)
    {
        _riverSegIndexHi = i;
    }

    public LineSegment GetRiverSegment(MapPolygon poly)
    {
        if (poly == HighId.Entity())
        {
            return HighSegsRel[_riverSegIndexHi];
        }
        else if (poly == LowId.Entity())
        {
            return LowSegsRel[LowSegsRel.Count - 1 - _riverSegIndexHi];
        }
        else throw new Exception("poly is not part of this border");
    }
}

public static class PolyBorderExt
{
    
    public static List<Vector2> GetPointsRel(this MapPolygonBorder b, MapPolygon p)
    {
        if (p == b.LowId.Entity()) return b.LowSegsRel.GetPoints().ToList();
        if (p == b.HighId.Entity()) return b.HighSegsRel.GetPoints().ToList();
        throw new Exception();
    }

    public static List<LineSegment> GetSegsRel(this MapPolygonBorder b, MapPolygon p)
    {
        if (p == b.LowId.Entity()) return b.LowSegsRel;
        if (p == b.HighId.Entity()) return b.HighSegsRel;
        throw new Exception();
    }
    public static List<LineSegment> GetSegsAbs(this MapPolygonBorder b, Data data)
    {
        return b.HighSegsRel
            .Select(s => s.Translate(b.HighId.Entity().Center))
            .ToList().OrderEndToStart(data);
    }
    public static Vector2 GetOffsetToOtherPoly(this MapPolygonBorder b, MapPolygon p)
    {
        var other = b.GetOtherPoly(p);
        var otherCount = b.GetSegsRel(other).Count;
        return b.GetSegsRel(p)[0].From - b.GetSegsRel(other)[otherCount - 1].To;
    }
    public static MapPolygon GetOtherPoly(this MapPolygonBorder b, MapPolygon p)
    {
        if (p == b.LowId.Entity()) return b.HighId.Entity();
        if (p == b.HighId.Entity()) return b.LowId.Entity();
        throw new Exception();
    }
    public static List<Vector2> GetPointsAbs(this MapPolygonBorder b)
    {
        return b.HighSegsRel.GetPoints().Select(p => p + b.HighId.Entity().Center).ToList();
    }

    public static bool IsRegimeBorder(this MapPolygonBorder b)
    {
        return b.HighId.Entity().Regime.RefId != b.LowId.Entity().Regime.RefId;
    }
    
}
