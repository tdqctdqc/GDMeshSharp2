using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class TempRiverData : Entity
{
    public Dictionary<PolyCornerKey, Vector2> Inners { get; private set; }
    public Dictionary<EdgeEndKey, Vector2> HiPivots { get; private set; }
    public Dictionary<MapPolygon, MapPolyRiverTriInfo> Infos { get; private set; }
    public static TempRiverData Construct(GenWriteKey key)
    {
        var r = new TempRiverData(new Dictionary<PolyCornerKey, Vector2>(), new Dictionary<EdgeEndKey, Vector2>(), key.IdDispenser.GetID());
        key.Create(r);
        return r;
    }
    private TempRiverData(Dictionary<PolyCornerKey, Vector2> inners, 
        Dictionary<EdgeEndKey, Vector2> hiPivots, int id) : base(id)
    {
        HiPivots = hiPivots;
        Inners = inners;
        Infos = new Dictionary<MapPolygon, MapPolyRiverTriInfo>();
    }

    public void GenerateInfos(GenWriteKey key)
    {
        var polys = key.Data.Planet.Polygons.Entities;
        var tempInfos = new ConcurrentDictionary<MapPolygon, MapPolyRiverTriInfo>();
        Parallel.ForEach(polys, poly =>
        {
            if (poly.GetEdges(key.Data).Any(e => e.IsRiver()) == false) return;
            var info = new MapPolyRiverTriInfo(poly, this, key);
            tempInfos.TryAdd(poly, info);
        });
        foreach (var kvp in tempInfos)
        {
            Infos.Add(kvp.Key, kvp.Value);
        }
    }
    
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(PlanetDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }

}

public class MapPolyRiverTriInfo
{
    public MapPolygon Poly { get; private set; }
    public Dictionary<EdgeEndKey, PolyTri> InnerTris { get; private set; }
    public Dictionary<MapPolygonEdge, List<PolyTri>> BankTris { get; private set; }
    public List<PolyTri> LandTris { get; private set; }
    public Dictionary<MapPolygonEdge, List<LineSegment>> BankSegs { get; private set; }
    public List<LineSegment> InnerBoundary { get; private set; }
    public MapPolyRiverTriInfo(MapPolygon poly, TempRiverData rData, GenWriteKey key)
    {
        Poly = poly;
        InnerTris = new Dictionary<EdgeEndKey, PolyTri>();
        BankTris = new Dictionary<MapPolygonEdge, List<PolyTri>>();
        BankSegs = new Dictionary<MapPolygonEdge, List<LineSegment>>();
        InnerBoundary = new List<LineSegment>();
        LandTris = new List<PolyTri>();
        var edges = poly.GetEdges(key.Data);
        if (edges.Any(e => e.IsRiver()) == false) return;
        var nexi = edges.Select(e => e.HiNexus.Entity())
            .Union(edges.Select(e2 => e2.LoNexus.Entity()))
            .Distinct()
            .Where(n => n.IncidentPolys.Contains(poly))
            .ToHashSet();
        
        MakePivotTris(key.Data, rData, nexi, edges);
        MakeBankTris(key.Data, rData, edges);
        MakeInnerBoundary(key.Data, rData, nexi, edges);
        MakeLandTris(rData, key);
    }


    private void MakePivotTris(Data data, TempRiverData rData, HashSet<MapPolyNexus> nexi,
        IEnumerable<MapPolygonEdge> edges)
    {
        foreach (var nexus in nexi)
        {
            if (nexus.IsRiverNexus() == false) continue;
            var nexusPoint = Poly.GetOffsetTo(nexus.Point, data);
            var nexusEdges = edges.Where(e => nexus.IncidentEdges.Contains(e));
            
            var innerKey = new PolyCornerKey(nexus, Poly);
            var inner = rData.Inners[innerKey];
            
            var edge1 = nexusEdges.ElementAt(0);
            if (edge1.IsRiver() == false)
            {
                var endKey1 = new EdgeEndKey(nexus, edge1);
                var pivot1 = GetPivot(endKey1, rData, data);
                InnerTris.Add(endKey1, PolyTri.Construct(nexusPoint, inner, pivot1, 
                    LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
            }

            if (nexusEdges.Count() > 1)
            {
                var edge2 = nexusEdges.ElementAt(1);
                if (edge2.IsRiver() == false)
                {
                    var endKey2 = new EdgeEndKey(nexus, edge2);
                    var pivot2 = GetPivot(endKey2, rData, data);
                    InnerTris.Add(endKey2, PolyTri.Construct(nexusPoint, inner, pivot2, 
                        LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
                }
            }
        }
    }
    private void MakeBankTris(Data data, TempRiverData rData,
        IEnumerable<MapPolygonEdge> edges)
    {
        foreach (var edge in edges)
        {
            if (edge.IsRiver() == false) continue;
            var width = River.GetWidthFromFlow(edge.MoistureFlow);
            
            var hiNexus = edge.HiNexus.Entity();
            var hiNexusPoint = Poly.GetOffsetTo(hiNexus.Point, data);
            var hiEnd = new EdgeEndKey(hiNexus, edge);
            var hiCorner = new PolyCornerKey(hiNexus, Poly);
            var hiInner = rData.Inners[hiCorner];
            
            var loNexus = edge.LoNexus.Entity();
            var loNexusPoint = Poly.GetOffsetTo(loNexus.Point, data);
            var loEnd = new EdgeEndKey(loNexus, edge);
            var loCorner = new PolyCornerKey(loNexus, Poly);
            var loInner = rData.Inners[loCorner];
            
            var edgeSegs = edge.GetSegsRel(Poly).Segments;
            var firstEdgeP = edgeSegs[0].From;
            var lastEdgeP = edgeSegs[edgeSegs.Count - 1].To;
            Vector2 startInner;
            Vector2 endInner;
            if (loNexusPoint == firstEdgeP && hiNexusPoint == lastEdgeP)
            {
                startInner = loInner;
                endInner = hiInner;
            }
            else if (hiNexusPoint == firstEdgeP && loNexusPoint == lastEdgeP)
            {
                startInner = hiInner;
                endInner = loInner;
            }
            else throw new Exception();
            
            var bankInnerPoints = new List<Vector2>();
            var edgeInnerSegs = new List<LineSegment>();
            
            for (var i = 0; i < edgeSegs.Count - 1; i++)
            {
                var thisSeg = edgeSegs[i];
                var nextSeg = edgeSegs[i + 1];
                if (thisSeg.To != nextSeg.From) throw new Exception();
                var thisSegAxis = (thisSeg.To - thisSeg.From).Normalized();
                var thisShift = thisSegAxis.Rotated(Mathf.Pi / 2f) * width / 2f;
                bankInnerPoints.Add(thisSeg.To + thisShift);
            }

            
            if (bankInnerPoints.Count > 0)
            {
                edgeInnerSegs.Add(new LineSegment(startInner, bankInnerPoints.First()));
                for (var i = 0; i < bankInnerPoints.Count - 1; i++)
                {
                    edgeInnerSegs.Add(new LineSegment(bankInnerPoints[i], bankInnerPoints[i + 1]));
                }
                edgeInnerSegs.Add(new LineSegment(bankInnerPoints.Last(), endInner));
            }
            else
            {
                edgeInnerSegs.Add(new LineSegment(startInner, endInner));
            }
            
            BankSegs.Add(edge, edgeInnerSegs);
            if (edgeSegs.Count != edgeInnerSegs.Count) throw new Exception();

            var bankTris = new List<PolyTri>();
            for (var i = 0; i < edgeSegs.Count; i++)
            {
                var outSeg = edgeSegs[i];
                var inSeg = edgeInnerSegs[i];
                bankTris.Add(PolyTri.Construct(outSeg.From, outSeg.To, inSeg.From,
                    LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
                bankTris.Add(PolyTri.Construct(outSeg.To, inSeg.To, inSeg.From,
                    LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
            }
            BankTris.Add(edge, bankTris);
        }
    }

    private void MakeInnerBoundary(Data data, TempRiverData rData, HashSet<MapPolyNexus> nexi,
        IEnumerable<MapPolygonEdge> edges)
    {
        var edgeInners = new List<List<LineSegment>>();

        foreach (var edge in edges)
        {
            var edgeInner = new List<LineSegment>();
            if (edge.IsRiver())
            {
                edgeInner.AddRange(BankSegs[edge]);
            }
            else if (edge.HiNexus.Entity().IsRiverNexus() == false && edge.LoNexus.Entity().IsRiverNexus() == false)
            {
                edgeInner.AddRange(edge.GetSegsRel(Poly).Segments);
            }
            else
            {
                var hiNexusP = Poly.GetOffsetTo(edge.HiNexus.Entity().Point, data);
                var loNexusP = Poly.GetOffsetTo(edge.LoNexus.Entity().Point, data);
                
                
                var edgeSegs = edge.GetSegsRel(Poly).Segments;

                MapPolyNexus fromNexus;
                MapPolyNexus toNexus;

                if (edgeSegs.Count > 1 && edgeSegs.First().From == edgeSegs[1].From)
                {
                    throw new Exception();
                }
                
                var from = edgeSegs.First().From;
                var to = edgeSegs.Last().To;
                if (hiNexusP == from
                    && loNexusP == to)
                {
                    fromNexus = edge.HiNexus.Entity();
                    toNexus = edge.LoNexus.Entity();
                }
                else if (hiNexusP == to
                         && loNexusP == from)
                {
                    toNexus = edge.HiNexus.Entity();
                    fromNexus = edge.LoNexus.Entity();
                } else { throw new Exception("bad epsilon"); }

                Vector2 continueFrom = from;
                Vector2 continueTo = to;
                
                LineSegment firstInnerEdge = null;
                var edgePoints = edgeSegs.GetPoints();
                var epsilon = .01f;
                if (fromNexus.IsRiverNexus())
                {
                    var fromInner = rData.Inners[new PolyCornerKey(fromNexus, Poly)];
                    var fromPivotSource = GetPivot(new EdgeEndKey(fromNexus, edge), rData, data);
                    var fromPivot = edgePoints.OrderBy(p => p.DistanceTo(fromPivotSource)).First();
                    
                    if (fromPivotSource.DistanceTo(fromPivot) > epsilon)
                    {
                        GD.Print("source pivot " + fromPivotSource);
                        GD.Print("closest pivot " + fromPivot);
                        throw new Exception();
                    }
                    
                    var fromPivotSeg = edgeSegs.First(ep => ep.To == fromPivot);
                    fromPivot = fromPivotSeg.To;
                    firstInnerEdge = new LineSegment(fromInner, fromPivot);
                    continueFrom = fromPivot;
                }

                LineSegment lastInnerEdge = null;
                if (toNexus.IsRiverNexus())
                {
                    var toInner = rData.Inners[new PolyCornerKey(toNexus, Poly)];
                    var toPivotSource = GetPivot(new EdgeEndKey(toNexus, edge), rData, data);
                    var toPivot = edgePoints.OrderBy(p => p.DistanceTo(toPivotSource)).First();
                    if (toPivotSource.DistanceTo(toPivot) > epsilon)
                    {
                        GD.Print("source pivot " + toPivotSource);
                        GD.Print("closest pivot " + toPivot);
                        throw new Exception();
                    }
                    var toPivotSeg = edgeSegs.First(ep => ep.From == toPivot);
                    lastInnerEdge = new LineSegment(toPivot, toInner);
                    continueTo = toPivot;
                }

                var continueFromIndex = edgeSegs.FindIndex(ls => ls.From == continueFrom);
                var continueToIndex = edgeSegs.FindIndex(ls => ls.To == continueTo);
                
                if(firstInnerEdge != null) edgeInner.Add(firstInnerEdge);
                for (var i = continueFromIndex; i <= continueToIndex; i++)
                {
                    edgeInner.Add(edgeSegs[i]);
                }
                if(lastInnerEdge != null && lastInnerEdge.IsSame(firstInnerEdge) == false)
                {
                    edgeInner.Add(lastInnerEdge);
                }
            }
            edgeInner = edgeInner.Chainify();
            edgeInners.Add(edgeInner);
        }

        try
        {
            InnerBoundary = edgeInners.Chainify();
        }
        catch
        {
            var e = new SegmentsException("failed to chainify inner river boundary");
            e.AddSegLayer(edgeInners.SelectMany(l => l).ToList(), "edge inners");

            int iter = 0;
            foreach (var eInner in edgeInners)
            {
                e.AddSegLayer(eInner, "edge inner " + iter++);

            }
            e.AddSegLayer(edges.SelectMany(b => b.GetSegsRel(Poly).Segments).ToList(), "poly edges");
            GD.Print(Poly.Center);
            throw e;
        }
        
        
        
        if (InnerBoundary[0].From != InnerBoundary[InnerBoundary.Count - 1].To)
        {
            InnerBoundary.Add(new LineSegment(InnerBoundary[InnerBoundary.Count - 1].To,
                InnerBoundary[0].From));
        }
        if (InnerBoundary.IsCircuit() == false)
        {
            var e = new SegmentsException("inner boundary not circuit");
            e.AddSegLayer(InnerBoundary, "inner");
            throw e;
        }
    }

    private void MakeLandTris(TempRiverData rData, GenWriteKey key)
    {
        var graph = new Graph<PolyTri, bool>();
        LandTris = InnerBoundary.TriangulateArbitrary(Poly, key, graph, true);
    }
    private Vector2 GetPivot(EdgeEndKey key, TempRiverData rData, Data data)
    {
        var pivot = rData.HiPivots[key];
        if (Poly != key.Edge.HighPoly.Entity())
        {
            pivot += Poly.GetOffsetTo(key.Edge.HighPoly.Entity(), data);
        }
        return pivot;
    }
    
}
