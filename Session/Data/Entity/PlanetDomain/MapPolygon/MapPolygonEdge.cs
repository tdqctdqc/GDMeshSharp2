using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;


public class MapPolygonEdge : Entity
{
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(PlanetDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public float MoistureFlow { get; protected set; }
    public PolyBorderChain LowSegsRel() => LowId.Entity().NeighborBorders[HighId.RefId];
    public PolyBorderChain HighSegsRel() => HighId.Entity().NeighborBorders[LowId.RefId];
    public EntityRef<MapPolygon> LowId { get; protected set; }
    public EntityRef<MapPolygon> HighId { get; protected set; }
    [SerializationConstructor] private MapPolygonEdge(int id, float moistureFlow, EntityRef<MapPolygon> lowId, 
        EntityRef<MapPolygon> highId) : base(id)
    {
        MoistureFlow = moistureFlow;
        LowId = lowId;
        HighId = highId;
    }
    // public static IEnumerable<MapPolygonEdge> CreateMany(
    //     List<KeyValuePair<PolyBorderChain, PolyBorderChain>> pairs, GenWriteKey key)
    // {
    //     var es = new List<MapPolygonEdge>(pairs.Count);
    //     for (var i = 0; i < pairs.Count; i++)
    //     {
    //         var p = pairs[i];
    //         var chain1 = p.Key;
    //         var chain2 = p.Value;
    //         var b = CreateInner(chain1, chain2, key);
    //         es.Add(b);
    //     }
    //     key.Create(es);
    //     return es;
    // }
    public static MapPolygonEdge Create(PolyBorderChain chain1, PolyBorderChain chain2, GenWriteKey key)
    {
        var b = CreateInner(chain1, chain2, key);
        key.Create(b);
        return b;
    }

    private static MapPolygonEdge CreateInner(PolyBorderChain chain1, PolyBorderChain chain2, GenWriteKey key)
    {
        EntityRef<MapPolygon> lowId;
        EntityRef<MapPolygon> highId;
        if (chain1.Native.Entity().Id < chain2.Native.Entity().Id)
        {
            lowId = new EntityRef<MapPolygon>(chain1.Native.Entity(), key);
            highId = new EntityRef<MapPolygon>(chain2.Native.Entity(), key);
        }
        else
        {
            lowId = new EntityRef<MapPolygon>(chain2.Native.Entity(), key);
            highId = new EntityRef<MapPolygon>(chain1.Native.Entity(), key);
        }
        
        PolyBorderChain lowChain = chain1.Native.Entity() == lowId.Entity() 
            ? chain1 : chain2;
        PolyBorderChain hiChain = chain1.Native.Entity() == lowId.Entity() 
            ? chain2 : chain1;
        lowId.Entity().AddNeighbor(highId.Entity(), lowChain, key);
        highId.Entity().AddNeighbor(lowId.Entity(), hiChain, key);
        var b = new MapPolygonEdge(
            key.IdDispenser.GetID(), 0f, lowId, highId);
        return b;
    }
    
    public static PolyBorderChain ConstructBorderChain(MapPolygon native, MapPolygon foreign, 
        List<LineSegment> segments, Data data)
    {
        var segsRel = RelativizeSegments(segments, native, data, out _);
        return PolyBorderChain.Construct(native, foreign, segsRel);
    }
    private static List<LineSegment> RelativizeSegments(List<LineSegment> abs, MapPolygon poly, Data data, out bool reversed)
    {
        reversed = false;
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
            reversed = true;
        }
        return res;
    }
    public void ReplacePoints(List<LineSegment> newSegmentsAbs, int riverSegIndex,
        GenWriteKey key)
    {
        var highBorderSegs = RelativizeSegments(newSegmentsAbs, HighId.Entity(), key.Data, out var hiRev);
        var lowBorderSegs = RelativizeSegments(newSegmentsAbs, LowId.Entity(), key.Data, out var loRev);

        var lowRiverIndex = -1;
        var hiRiverIndex = -1;
        if (riverSegIndex != -1)
        {
            lowRiverIndex = loRev ? newSegmentsAbs.Count - riverSegIndex - 1 : riverSegIndex;
            hiRiverIndex = hiRev ? newSegmentsAbs.Count - riverSegIndex - 1 : riverSegIndex;
        }
        
        var lowSegsRel = PolyBorderChain.ConstructRiver(LowId.Entity(), HighId.Entity(), 
            lowBorderSegs, lowRiverIndex);
        var highSegsRel = PolyBorderChain.ConstructRiver(HighId.Entity(), LowId.Entity(), 
            highBorderSegs, hiRiverIndex);
        
        HighId.Entity().SetNeighborBorder(LowId.Entity(), highSegsRel, key);
        LowId.Entity().SetNeighborBorder(HighId.Entity(), lowSegsRel, key);
    }
    
    public void SetFlow(float width, GenWriteKey key)
    {
        MoistureFlow = width;
    }
    public void IncrementFlow(float increment, GenWriteKey key)
    {
        MoistureFlow += increment;
    }

    public bool HasRiver()
    {
        return HighSegsRel().HasRiver();
    }
    public LineSegment GetRiverSegment(MapPolygon poly)
    {
        if (poly == HighId.Entity())
        {
            return HighId.Entity().GetBorder(LowId.RefId).GetRiverSegment();
        }
        else if (poly == LowId.Entity())
        {
            return LowId.Entity().GetBorder(HighId.RefId).GetRiverSegment();
        }
        else throw new Exception("poly is not part of this border");
    }
}

