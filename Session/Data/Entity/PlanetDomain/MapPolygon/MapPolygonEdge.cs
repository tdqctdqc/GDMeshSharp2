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
    public PolyBorderChain LowSegsRel() => LowPoly.Entity().NeighborBorders[HighPoly.RefId];
    public PolyBorderChain HighSegsRel() => HighPoly.Entity().NeighborBorders[LowPoly.RefId];
    public EntityRef<MapPolygon> LowPoly { get; protected set; }
    public EntityRef<MapPolygon> HighPoly { get; protected set; }
    public Dictionary<byte, byte> HiToLoTriPaths { get; private set; }
    public Dictionary<byte, byte> LoToHiTriPaths { get; private set; }
    public EntityRef<MapPolyNexus> HiNexus { get; private set; }
    public EntityRef<MapPolyNexus> LoNexus { get; private set; }
    [SerializationConstructor] private MapPolygonEdge(int id, float moistureFlow, 
        EntityRef<MapPolygon> lowPoly, EntityRef<MapPolygon> highPoly, Dictionary<byte, byte> hiToLoTriPaths,
        Dictionary<byte, byte> loToHiTriPaths, EntityRef<MapPolyNexus> loNexus, EntityRef<MapPolyNexus> hiNexus) 
        : base(id)
    {
        HiToLoTriPaths = hiToLoTriPaths;
        LoToHiTriPaths = loToHiTriPaths;
        MoistureFlow = moistureFlow;
        LowPoly = lowPoly;
        HighPoly = highPoly;
        LoNexus = loNexus;
        HiNexus = hiNexus;
    }
    public static MapPolygonEdge Create(PolyBorderChain chain1, PolyBorderChain chain2, GenWriteKey key)
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
            key.IdDispenser.GetID(), 0f, lowId, highId,
            new Dictionary<byte, byte>(), new Dictionary<byte, byte>(),
            null, null);
        key.Create(b);
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
    public void ReplacePoints(List<LineSegment> newSegmentsAbs,
        GenWriteKey key)
    {
        var highBorderSegs = RelativizeSegments(newSegmentsAbs, HighPoly.Entity(), key.Data, out var hiRev);
        var lowBorderSegs = RelativizeSegments(newSegmentsAbs, LowPoly.Entity(), key.Data, out var loRev);
        
        var lowSegsRel = PolyBorderChain.Construct(LowPoly.Entity(), HighPoly.Entity(), 
            lowBorderSegs);
        var highSegsRel = PolyBorderChain.Construct(HighPoly.Entity(), LowPoly.Entity(), 
            highBorderSegs);
        
        HighPoly.Entity().SetNeighborBorder(LowPoly.Entity(), highSegsRel, key);
        LowPoly.Entity().SetNeighborBorder(HighPoly.Entity(), lowSegsRel, key);
    }
    
    public void SetFlow(float width, GenWriteKey key)
    {
        MoistureFlow = width;
    }
    public void IncrementFlow(float increment, GenWriteKey key)
    {
        MoistureFlow += increment;
    }

    public void SetNexi(MapPolyNexus n1, MapPolyNexus n2, GenWriteKey key)
    {
        HiNexus = n1.MakeRef();
        LoNexus = n2.MakeRef();
    }
}

