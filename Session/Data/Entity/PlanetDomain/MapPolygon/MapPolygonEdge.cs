using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;


public class MapPolygonEdge : Entity, IBorderChain<LineSegment, MapPolygon>
{
    public override Type GetDomainType() => typeof(PlanetDomain);
    public float MoistureFlow { get; private set; }
    public PolyBorderChain SegsAbs { get; private set; }
    public PolyBorderChain LowSegsRel { get; private set; }
    public PolyBorderChain HighSegsRel { get; private set; }
    public EntityRef<MapPolygon> LowId { get; private set; }
    public EntityRef<MapPolygon> HighId { get; private set; }
    private int _riverSegIndexHi = -1;

    [SerializationConstructor] private MapPolygonEdge(int id, float moistureFlow, PolyBorderChain lowSegsRel, 
        PolyBorderChain highSegsRel, EntityRef<MapPolygon> lowId, 
        EntityRef<MapPolygon> highId) : base(id)
    {
        MoistureFlow = moistureFlow;
        LowSegsRel = lowSegsRel;
        HighSegsRel = highSegsRel;
        if (lowSegsRel.Count != highSegsRel.Count) throw new Exception();
        LowId = lowId;
        HighId = highId;
    }

    public static MapPolygonEdge Create(int id, MapPolygon poly1, MapPolygon poly2, 
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
        
        var highSegsRel = RelativizeSegments(segments, highId.Entity(), key.Data);
        var lowSegsRel = RelativizeSegments(segments, lowId.Entity(), key.Data);
        var lowBorder = PolyBorderChain.Construct(lowId.Entity(), highId.Entity(), lowSegsRel);
        var highBorder = PolyBorderChain.Construct(highId.Entity(), lowId.Entity(), highSegsRel);
        
        var b =  new MapPolygonEdge(
            id, 0f, lowBorder, highBorder, lowId, highId);
        poly1.AddNeighbor(poly2, b, key);
        poly2.AddNeighbor(poly1, b, key);
        key.Create(b);
        return b;
    }

    private static List<LineSegment> RelativizeSegments(List<LineSegment> abs, MapPolygon poly, Data data)
    {
        var res = new List<LineSegment>();
        for (var i = 0; i < abs.Count; i++)
        {
            var absSeg = abs[i];
            var t = poly.GetOffsetTo(absSeg.To, data);
            var f = poly.GetOffsetTo(absSeg.From, data);
            var relSeg = Clockwise.IsClockwise(t, f, Vector2.Zero)
                ? new LineSegment(t, f)
                : new LineSegment(f, t);
            res.Add(relSeg);
        }

        if (res.IsContinuous() == false)
        {
            res.Reverse();
        }
        return res;
    }
    public void ReplacePoints(List<LineSegment> newSegmentsAbs, 
        GenWriteKey key)
    {
        var highBorderSegs = RelativizeSegments(newSegmentsAbs, HighId.Entity(), key.Data);
        var lowBorderSegs = RelativizeSegments(newSegmentsAbs, LowId.Entity(), key.Data);
        LowSegsRel = PolyBorderChain.Construct(LowId.Entity(), HighId.Entity(), lowBorderSegs);
        HighSegsRel = PolyBorderChain.Construct(HighId.Entity(), LowId.Entity(), highBorderSegs);
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

    MapPolygon IBorder<MapPolygon>.Native => HighId.Entity();
    MapPolygon IBorder<MapPolygon>.Foreign => LowId.Entity();
    IReadOnlyList<LineSegment> IChain<LineSegment>.Elements => ((IChain<LineSegment>)SegsAbs).Elements;
}

