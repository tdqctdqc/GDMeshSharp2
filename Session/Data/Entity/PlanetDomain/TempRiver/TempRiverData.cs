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
            var info = new MapPolyRiverTriInfo(poly, this, key.Data);
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
    public Dictionary<MapPolygonEdge, List<LineSegment>> BankSegs { get; private set; }
    public List<Triangle> LandTris { get; private set; }
    public List<LineSegment> InnerBoundary { get; private set; }
    public MapPolyRiverTriInfo(MapPolygon poly, TempRiverData rData, Data data)
    {
        Poly = poly;
        InnerTris = new Dictionary<EdgeEndKey, PolyTri>();
        BankTris = new Dictionary<MapPolygonEdge, List<PolyTri>>();
        BankSegs = new Dictionary<MapPolygonEdge, List<LineSegment>>();
        InnerBoundary = new List<LineSegment>();
        var edges = poly.GetEdges(data);
        if (edges.Any(e => e.IsRiver()) == false) return;
        var nexi = edges.Select(e => e.HiNexus.Entity())
            .Union(edges.Select(e2 => e2.LoNexus.Entity()))
            .Distinct()
            .Where(n => n.IncidentPolys.Contains(poly))
            .ToHashSet();
        
        MakePivotTris(data, rData, nexi, edges);
        MakeBankTris(data, rData, edges);
        MakeLandTris(data, rData, nexi, edges);
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
            var endKey1 = new EdgeEndKey(nexus, edge1);
            var pivot1 = rData.HiPivots[endKey1];
            if (Poly != edge1.HighPoly.Entity())
            {
                pivot1 += Poly.GetOffsetTo(edge1.HighPoly.Entity(), data);
            }

            var edge2 = nexusEdges.ElementAt(1);
            var endKey2 = new EdgeEndKey(nexus, edge2);
            var pivot2 = rData.HiPivots[endKey2];
            if (Poly != edge2.HighPoly.Entity())
            {
                pivot2 += Poly.GetOffsetTo(edge2.HighPoly.Entity(), data);
            }
            
            InnerTris.Add(endKey1, PolyTri.Construct(nexusPoint, inner, pivot1, 
                LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
            InnerTris.Add(endKey2, PolyTri.Construct(nexusPoint, inner, pivot2, 
                LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
        }
    }
    private void MakeBankTris(Data data, TempRiverData rData,
        IEnumerable<MapPolygonEdge> edges)
    {

        foreach (var edge in edges)
        {
            if (edge.IsRiver())
            {
                var hiNexus = edge.HiNexus.Entity();
                var hiNexusPoint = Poly.GetOffsetTo(hiNexus.Point, data);
                var hiEnd = new EdgeEndKey(hiNexus, edge);
                var hiCorner = new PolyCornerKey(hiNexus, Poly);
                var hiInner = rData.Inners[hiCorner];
                var hiPivot = GetPivot(hiEnd, rData, data);
                
                var loNexus = edge.LoNexus.Entity();
                var loNexusPoint = Poly.GetOffsetTo(loNexus.Point, data);
                var loEnd = new EdgeEndKey(loNexus, edge);
                var loCorner = new PolyCornerKey(loNexus, Poly);
                var loInner = rData.Inners[loCorner];
                var loPivot = GetPivot(loEnd, rData, data);
                
                List<Vector2> edgePoints;
                var bankTris = new List<PolyTri>();
                if (Poly == edge.HighPoly.Entity())
                {
                    edgePoints = edge.HighSegsRel().Segments.GetPoints().ToList();
                }
                else
                {
                    edgePoints = edge.LowSegsRel().Segments.GetPoints().ToList();
                }
                
                hiPivot = edgePoints.OrderBy(ep => ep.DistanceTo(hiPivot)).First();
                loPivot = edgePoints.OrderBy(ep => ep.DistanceTo(loPivot)).First();

                var hiPivI = edgePoints.IndexOf(hiPivot);
                var loPivI = edgePoints.IndexOf(loPivot);

                if (hiPivI == -1 || loPivI == -1)
                {
                    throw new Exception();
                }

                if (hiPivI < loPivI)
                {
                    hiPivI = edgePoints.Count - (1 + hiPivI);
                    loPivI = edgePoints.Count - (1 + loPivI);
                    edgePoints.Reverse();
                }
                var bankOuterPoints = new List<Vector2>();
                for (var i = loPivI + 1; i <= hiPivI - 1; i++)
                {
                    bankOuterPoints.Add(edgePoints[i]);
                }

                var bankInnerPoints = bankOuterPoints
                    .Select(o => o * .9f).ToList();
                var edgeBankSegs = new List<LineSegment>();
                
                
                if (bankOuterPoints.Count > 0)
                {
                    edgeBankSegs.Add(new LineSegment(loInner, bankInnerPoints.First()));
                    for (var i = 0; i < bankInnerPoints.Count - 1; i++)
                    {
                        edgeBankSegs.Add(new LineSegment(bankInnerPoints[i], bankInnerPoints[i + 1]));
                    }
                    edgeBankSegs.Add(new LineSegment(bankInnerPoints.Last(), hiInner));
                }
                else
                {
                    edgeBankSegs.Add(new LineSegment(loInner, hiInner));
                }
                BankSegs.Add(edge, edgeBankSegs);

                if (bankOuterPoints.Count > 0)
                {
                    bankTris.Add(PolyTri.Construct(loInner, loPivot, bankOuterPoints[0], 
                        LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
                    bankTris.Add(PolyTri.Construct(loInner, bankInnerPoints[0], bankOuterPoints[0], 
                        LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
                    for (var i = 0; i < bankOuterPoints.Count - 1; i++)
                    {
                        var o1 = bankOuterPoints[i];
                        var o2 = bankOuterPoints[i + 1];
                        var i1 = bankInnerPoints[i];
                        var i2 = bankInnerPoints[i + 1];
                        bankTris.Add(PolyTri.Construct(o1, o2, i1, 
                            LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
                        bankTris.Add(PolyTri.Construct(i2, o2, i1, 
                            LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
                    }
                    bankTris.Add(PolyTri.Construct(hiInner, hiPivot, bankOuterPoints.Last(), 
                        LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
                    bankTris.Add(PolyTri.Construct(hiInner, bankInnerPoints.Last(), bankOuterPoints.Last(), 
                        LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
                    BankTris.Add(edge, bankTris);
                }
                else
                {
                    bankTris.Add(PolyTri.Construct(loInner, loPivot, hiInner, 
                        LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
                    bankTris.Add(PolyTri.Construct(loPivot, hiPivot, hiInner, 
                        LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
                    BankTris.Add(edge, bankTris);
                }
            }
        }
    }

    private void MakeLandTris(Data data, TempRiverData rData, HashSet<MapPolyNexus> nexi,
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
                var badFrom = first.From;
                first.From = first.To;
                first.To = badFrom;
            }
            
            var from = edgeSegs.First().From;
            var to = edgeSegs.Last().To;
            var epsilon = .1f;
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
                
                
                
                
                GD.Print("loNexus rel " + loNexusP);
                GD.Print("hiNexus rel " + hiNexusP);
                GD.Print("hi to to " + hiNexusP.DistanceTo(to));
                GD.Print("hi to from " + hiNexusP.DistanceTo(from));
                GD.Print("lo to to " + loNexusP.DistanceTo(to));
                GD.Print("lo to from " + loNexusP.DistanceTo(from));
                for (int i = 0; i < edge.GetSegsRel(Poly).Segments.Count; i++)
                {
                    GD.Print(edge.GetSegsRel(Poly).Segments[i]);
                }
                throw new Exception();
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
                if (seg.From == continueFrom) add = true;
                if(add) InnerBoundary.Add(seg);
                if (seg.To == continueTo) break;
            }
            
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
    private struct LineSegmentStruct
    {
        public Vector2 From { get; private set; }
        public Vector2 To { get; private set; }

        public LineSegmentStruct(LineSegment ls)
        {
            From = ls.From;
            To = ls.To;
        }

        public LineSegmentStruct(Vector2 from, Vector2 to)
        {
            From = from;
            To = to;
        }

        public LineSegmentStruct Reverse()
        {
            return new LineSegmentStruct(To, From);
        }
    }
}
