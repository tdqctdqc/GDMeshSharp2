using System;
using System.Collections.Generic;
using System.Linq;
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
        foreach (var poly in key.Data.Planet.Polygons.Entities)
        {
            if (poly.GetEdges(key.Data).Any(e => e.IsRiver()) == false) continue;
            var info = new MapPolyRiverTriInfo(poly, this, key);
            Infos.Add(poly, info);
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
            if (nexusEdges.Count() != 2)
            {
                throw new Exception();
                continue;
            }
            

            
            var innerKey = new PolyCornerKey(nexus, Poly);
            var inner = rData.Inners[innerKey];
            var edge1 = nexusEdges.ElementAt(0);

            if (edge1.IsRiver() == false)
            {
                var endKey1 = new EdgeEndKey(nexus, edge1);
                var pivot1 = rData.HiPivots[endKey1];
                if (Poly != edge1.HighPoly.Entity())
                {
                    pivot1 += Poly.GetOffsetTo(edge1.HighPoly.Entity(), data);
                }
                InnerTris.Add(endKey1, PolyTri.Construct(nexusPoint, inner, pivot1, 
                    LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
            }
            
            var edge2 = nexusEdges.ElementAt(1);


            if (edge2.IsRiver() == false)
            {
                var endKey2 = new EdgeEndKey(nexus, edge2);
                var pivot2 = rData.HiPivots[endKey2];
                if (Poly != edge2.HighPoly.Entity())
                {
                    pivot2 += Poly.GetOffsetTo(edge2.HighPoly.Entity(), data);
                }
                InnerTris.Add(endKey2, PolyTri.Construct(nexusPoint, inner, pivot2, 
                    LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
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
            
            var edgePoints = edge.GetSegsRel(Poly).Segments.GetPoints().ToList();
            Vector2 startInner;
            Vector2 endInner;
            if (loNexusPoint == edgePoints.First() && hiNexusPoint == edgePoints.Last())
            {
                startInner = loInner;
                endInner = hiInner;
            }
            else if (hiNexusPoint == edgePoints.First() && loNexusPoint == edgePoints.Last())
            {
                startInner = hiInner;
                endInner = loInner;
            }
            else throw new Exception();

            var edgeSegs = edge.GetSegsRel(Poly).Segments;
            if (edgeSegs.Any(e => e.IsCCW(Vector2.Zero))) throw new Exception();
            if (edgeSegs.Any(e => e.From == e.To)) throw new Exception();
            if (edgeSegs.IsChain() == false) throw new Exception();
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
        InnerBoundary = new List<LineSegment>();

        foreach (var edge in edges)
        {
            if (edge.IsRiver())
            {
                InnerBoundary.AddRange(BankSegs[edge]);
                continue;
            }
            if (edge.HiNexus.Entity().IsRiverNexus() == false && edge.LoNexus.Entity().IsRiverNexus() == false)
            {
                InnerBoundary.AddRange(edge.GetSegsRel(Poly).Segments);
                continue;
            }

            var hiNexusP = Poly.GetOffsetTo(edge.HiNexus.Entity().Point, data);
            var loNexusP = Poly.GetOffsetTo(edge.LoNexus.Entity().Point, data);
            
            
            var edgeSegs = edge.GetSegsRel(Poly).Segments;

            MapPolyNexus fromNexus;
            MapPolyNexus toNexus;

            var first = edgeSegs.First();
            if (edgeSegs.Count > 1 && first.From == edgeSegs[1].From)
            {
                GD.Print("short fix");
                var temp = first.From;
                first.From = first.To;
                first.To = temp;
            }
            
            var from = edgeSegs.First().From;
            var to = edgeSegs.Last().To;
            var epsilon = 1f;
            if (hiNexusP.DistanceTo(from) <= epsilon 
                && loNexusP.DistanceTo(to) <= epsilon)
            {
                fromNexus = edge.HiNexus.Entity();
                toNexus = edge.LoNexus.Entity();
            }
            else if (hiNexusP.DistanceTo(to) <= epsilon 
                     && loNexusP.DistanceTo(from) <= epsilon)
            {
                toNexus = edge.HiNexus.Entity();
                fromNexus = edge.LoNexus.Entity();
            }
            else
            {
                throw new Exception("bad epsilon");
            }

            Vector2 continueFrom = from;
            Vector2 continueTo = to;
            if (fromNexus.IsRiverNexus())
            {
                var fromInner = rData.Inners[new PolyCornerKey(fromNexus, Poly)];
                var fromPivot = GetPivot(new EdgeEndKey(fromNexus, edge), rData, data);
                var fromPivotSeg = edgeSegs.OrderBy(ep => ep.From.DistanceTo(fromPivot)).First();
                fromPivot = fromPivotSeg.From;
                InnerBoundary.Add(new LineSegment(fromInner, fromPivot));
                continueFrom = fromPivot;
            }
            
            if (toNexus.IsRiverNexus())
            {
                var toInner = rData.Inners[new PolyCornerKey(toNexus, Poly)];
                var toPivot = GetPivot(new EdgeEndKey(toNexus, edge), rData, data);
                var toPivotSeg = edgeSegs.OrderBy(ep => ep.To.DistanceTo(toPivot)).First();
                toPivot = toPivotSeg.To;
                InnerBoundary.Add(new LineSegment(toPivot, toInner));
                continueTo = toPivot;
            }

            bool add = false;
            for (var i = 0; i < edgeSegs.Count; i++)
            {
                var seg = edgeSegs[i];
                if (seg.From == seg.To) continue;
                if (seg.From == continueFrom) add = true;
                if(add) InnerBoundary.Add(seg);
                if (seg.To == continueTo) break;
            }
        }
        InnerBoundary = InnerBoundary.Circuitify();
    }

    private void MakeLandTris(TempRiverData rData, GenWriteKey key)
    {
        var graph = new Graph<PolyTri, bool>();
        if (InnerBoundary.First().IsCCW(Vector2.Zero))
        {
            InnerBoundary = InnerBoundary.Select(ls => ls.Reverse()).Reverse().ToList();
            LandTris = InnerBoundary.TriangulateArbitrary(Poly, key, graph, true);
        }
        else
        {
            LandTris = InnerBoundary.TriangulateArbitrary(Poly, key, graph, true);
        }
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
