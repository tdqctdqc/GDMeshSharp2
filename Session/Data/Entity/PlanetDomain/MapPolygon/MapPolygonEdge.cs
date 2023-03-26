using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;


public class MapPolygonEdge : Entity
{
    public override Type GetRepoEntityType() => RepoEntityType();
    private static Type RepoEntityType() => typeof(MapPolygonEdge);
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(PlanetDomain);
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

    public static MapPolygonEdge Create(MapPolygon poly1, MapPolygon poly2, 
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
        
        var highSegsRel = RelativizeSegments(segments, highId.Entity(), key.Data, out _);
        var lowSegsRel = RelativizeSegments(segments, lowId.Entity(), key.Data, out _);
        var lowBorder = PolyBorderChain.Construct(lowId.Entity(), highId.Entity(), lowSegsRel);
        var highBorder = PolyBorderChain.Construct(highId.Entity(), lowId.Entity(), highSegsRel);
        
        
        lowId.Entity().AddNeighbor(highId.Entity(), lowBorder, key);
        highId.Entity().AddNeighbor(lowId.Entity(), highBorder, key);
        var b =  new MapPolygonEdge(
            key.IdDispenser.GetID(), 0f, lowId, highId);

        key.Create(b);
        return b;
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

