using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Poly2Tri;
using Poly2Tri.Triangulation.Polygon;
using Poly2Tri.Triangulation.Sets;
using Poly2Tri.Utility;

public class PolyTriGenerator
{
    private Dictionary<MapPolygonBorder, int> _riverBorders;
    private Data _data;
    public void BuildTris(GenWriteKey key)
    {
        _data = key.Data;
        _riverBorders = new Dictionary<MapPolygonBorder, int>();
        var polys = key.Data.Planet.Polygons.Entities;
        FindAndSizeRiverSegs(key);
        foreach (var p in polys)
        {
            BuildTris(p, key);
        }
    }
    
    private void FindAndSizeRiverSegs(GenWriteKey key)
    {
        var polys = key.Data.Planet.Polygons.Entities;
        var borders = key.Data.Planet.PolyBorders.Entities;
        
        _riverBorders = borders.Where(b => b.MoistureFlow > River.FlowFloor).ToDictionary(s => s, s => -1);
        
        var logBase = Mathf.Pow(River.FlowCeil, 1f / (River.WidthCeil - River.WidthFloor));
        
        var max = 0f;
        var min = Mathf.Inf;
        var avg = 0f;

        foreach (var rBorder in _riverBorders.Keys.ToList())
        {
            var hi = rBorder.HighId.Ref();

            var flow = rBorder.MoistureFlow;
            if (flow > River.FlowCeil)
            {
                throw new Exception($"flow is {flow} too wide for ceiling {River.FlowCeil}");
            }
            var width = River.FlowFloor + (float)Math.Log(flow, logBase);
            max = Mathf.Max(max, width);
            min = Mathf.Min(min, width);
            avg += width;

            var segs = rBorder.GetSegsAbs(_data);
            var numSegs = segs.Count;

            var start = segs[0].From;
            var end = segs.Last().To;
            var axis = (end - start).Normalized();
            var length = start.DistanceTo(end);
            if (length > 1000f) throw new Exception();
            var mid = (start + end) / 2f;

            if (length < 2f * width)
            {
                width = length / 2f;
            }
            var p1 = mid - axis * width / 2f;
            var preSegs = p1.GeneratePointsAlong(50f, 10f, true, null, start).GetLineSegments();

            var p2 = mid + axis * width / 2f;
            var postSegs = end.GeneratePointsAlong(50f, 10f, true, null, p2).GetLineSegments();
            var rSeg = new LineSegment(p1, p2);
            var newSegs = preSegs.ToList();
            newSegs.Add(rSeg);
            newSegs.AddRange(postSegs);
            newSegs = newSegs.OrderEndToStart(_data, rBorder.HighId.Ref());
            
            if(newSegs.IsContinuous() == false)
            {
                throw new SegmentsNotConnectedException(key.GenData, hi, hi.BorderSegments.ToList(), newSegs, null);
            }
            
            var newIndex = preSegs.Count();
            rBorder.ReplacePoints(newSegs, 
                key);
            _riverBorders[rBorder] = newIndex;
            rBorder.SetRiverIndexHi(newIndex, key);
        }
        foreach (var p in _data.Planet.Polygons.Entities)
        {
            p.SetBorderSegments(key);
        }

    }

    private void BuildTris(MapPolygon poly, GenWriteKey key)
    {
        if (poly.IsWater())
        {
            DoSeaPoly(poly, key);
        }
        else if (poly.Neighbors.Refs().Any(n => _riverBorders.ContainsKey(poly.GetBorder(n, key.Data))))
        {
            DoRiverPoly(poly, key);
        }
        else
        {
            DoLandPolyNoRivers(poly, key);
        }
    }

    private void DoSeaPoly(MapPolygon poly, GenWriteKey key)
    {
        var borderSegsRel = poly.BorderSegments;
        var lf = key.Data.Models.Landforms;
        var v = key.Data.Models.Vegetation;
        var points = new List<Vector2> {Vector2.Zero};
        points.AddRange(borderSegsRel.GetPoints());
        
        var tris = DelaunayTriangulator.TriangulatePoints(points)
            .Select(t =>
            {
                return new PolyTri(t.A, t.B, t.C, LandformManager.Sea, VegetationManager.Barren);
            })
            .ToList();
        var polyTerrainTris = PolyTerrainTris.Construct(
            tris.ToList(), 
            key.Data);
        poly.SetTris(polyTerrainTris, key);
    }
    
    private void DoLandPolyNoRivers(MapPolygon poly, GenWriteKey key)
    {
        var borderSegsRel = poly.BorderSegments;
        var lf = key.Data.Models.Landforms;
        var v = key.Data.Models.Vegetation;
        var points = borderSegsRel.GenerateInteriorPoints(50f, 10f)
            .ToList();
        points.AddRange(borderSegsRel.GetPoints());
        
        var tris = DelaunayTriangulator.PolyTriangulatePoints(poly, points, _data);
        var polyTerrainTris = PolyTerrainTris.Construct(
            tris.ToList(), 
            key.Data);
        poly.SetTris(polyTerrainTris, key);
    }
    private void DoRiverPoly(MapPolygon poly, GenWriteKey key)
    {
        var lf = key.Data.Models.Landforms;
        var v = key.Data.Models.Vegetation;
        var tris = new List<PolyTri>();
        var borders = poly.Neighbors.Refs().Select(n => poly.GetBorder(n, key.Data));
        var riverSegs = borders
            .Where(b => _riverBorders.ContainsKey(b))
            .Select(b => b.GetRiverSegment(poly)).ToList();
        riverSegs.OrderByClockwise(Vector2.Zero, ls => ls.From);
        var borderSegsRel = poly.BorderSegments;
        var firstRSegIndex = borderSegsRel.IndexOf(riverSegs.First());
        var firstRSeg = riverSegs.First();
        
        if (riverSegs.Count == 1)
        {
            DoRiverSource(poly, borderSegsRel, firstRSeg, tris, key);
        }
        else
        {
            DoRiverJunction(poly, borderSegsRel, riverSegs, firstRSeg, firstRSegIndex, tris, key);
        }
        var polyTerrainTris = PolyTerrainTris.Construct(
            tris.ToList(), 
            key.Data);
        poly.SetTris(polyTerrainTris, key);
    }

    private void DoRiverSource(MapPolygon poly, IReadOnlyList<LineSegment> borderSegs, LineSegment rSeg, 
        List<PolyTri> tris, GenWriteKey key)
    {
        var rSegIndex = borderSegs.IndexOf(rSeg);
        var between = Enumerable.Range(1, borderSegs.Count - 1)
            .Select(i => borderSegs.Modulo(rSegIndex + i))
            .ToList();
        // if (between.IsClockwise() == false)
        // {
        //     between.OrderByClockwise(Vector2.Zero, ls => ls.From);
        // }
        if (between.Count != borderSegs.Count - 1) throw new Exception();
        if (between.Contains(rSeg)) throw new Exception();
        if (between.First().From != rSeg.To || between.Last().To != rSeg.From)
        {
            GD.Print($"{between.First().From} between first");
            GD.Print($"{between.Last().To} between last");
            GD.Print($"rSeg is {rSeg.ToString()}");
            throw new SegmentsNotConnectedException(_data, poly, poly.BorderSegments.ToList(), 
                between, 
                new List<LineSegment>{rSeg});
        }
        if (between.Contains(rSeg)) throw new Exception();

        var rTri = new PolyTri(rSeg.From, rSeg.To, Vector2.Zero, LandformManager.River, VegetationManager.Barren);
        tris.Add(rTri);
        var outline = GetOutline(poly, Vector2.Zero, between);
        TriangulateArbitrary(poly, outline, tris, key);
    }

    private void DoRiverJunction(MapPolygon poly, IReadOnlyList<LineSegment> borderSegsRel, 
        List<LineSegment> riverSegs,
        LineSegment firstRSeg,
        int firstRSegIndex,
        List<PolyTri> tris, GenWriteKey key)
    {
        
        
        var junctionPoints = new List<Vector2>();
        var lf = key.Data.Models.Landforms;
        var veg = key.Data.Models.Vegetation;
        
        for (var i = 0; i < riverSegs.Count; i++)
        {
            var rSeg = riverSegs[i];
            var nextRSeg = riverSegs.Next(i);
            
            var nextBetween = nextRSeg.Mid().Rotated(nextRSeg.Mid().GetClockwiseAngleTo(rSeg.Mid()) / 2f);
            var nextL = (rSeg.Length() + nextRSeg.Length()) / 2f;
            var nextIntersect = 
                nextBetween.Normalized() * nextL / 2f;

            if (riverSegs.Count == 2)
            {
                tris.Add(new PolyTri(rSeg.From, rSeg.To, nextIntersect,
                    LandformManager.River, VegetationManager.Barren));
                tris.Add(new PolyTri(rSeg.From, nextIntersect, -nextIntersect,
                    LandformManager.River, VegetationManager.Barren));
                junctionPoints.Add(nextIntersect);
            }
            else
            {
                tris.Add(new PolyTri(nextIntersect, Vector2.Zero, rSeg.From,
                    LandformManager.River, VegetationManager.Barren));
                tris.Add(new PolyTri(rSeg.To, nextIntersect, rSeg.From,
                    LandformManager.River, VegetationManager.Barren));
                tris.Add(new PolyTri(nextRSeg.From, nextIntersect, Vector2.Zero,
                    LandformManager.River, VegetationManager.Barren));
                junctionPoints.Add(nextIntersect);
            }
        }
        
       
        
        var rHash = new HashSet<LineSegment>(riverSegs);
        var prevR = firstRSeg;
        var prevRIndex = firstRSegIndex;
        var between = new List<LineSegment>();
        int iter = 0;
        int rIter = 0;
        for (int i = 1; i < borderSegsRel.Count + 1; i++)
        {
            var seg = borderSegsRel.Modulo(firstRSegIndex + i);
            if (rHash.Contains(seg))
            {
                var intersect = junctionPoints.Modulo(rIter);
                rIter++;
                
                if (between.Count == 0)
                {
                    GD.Print($"0 border at {poly.Center} {poly.Id}");
                    //this is happening when two incoming river segs share a point
                    continue;
                }
                if (between.IsClockwise() == false)
                {
                    between.OrderByClockwise(Vector2.Zero, ls => ls.From);
                }
                var outline = GetOutline(poly, intersect, between);
                TriangulateArbitrary(poly, outline, tris, key);
                
                
                between.Clear();
                prevR = seg;
                iter = 0;
            }
            else
            {
                between.Add(seg);
            }
        }
    }

    private void TriangulateArbitrary(MapPolygon poly, List<LineSegment> outline, List<PolyTri> tris, GenWriteKey key)
    {
        var lf = key.Data.Models.Landforms;
        var veg = key.Data.Models.Vegetation;

        var interior = outline.GenerateInteriorPoints(50f, 10f).ToHashSet();
        tris.AddRange(outline.PolyTriangulate(key.GenData, poly, interior));
    }
    private Vector2 GetIntersection(LineSegment from, LineSegment to)
    {
        if (to.Mid().Normalized() == -from.Mid().Normalized())
        {
            throw new Exception();
        }
            
        var has = Vector2Ext.GetLineIntersection(to.From, to.From - to.Mid(),
            from.To, from.To - from.Mid(), 
            out var intersect);

        if (Mathf.IsNaN(intersect.x) || Mathf.IsNaN(intersect.y)) throw new Exception();

        return intersect;
    }

    private List<LineSegment> GetOutline(MapPolygon poly, Vector2 fanPoint, List<LineSegment> between)
    {
        List<LineSegment> generateBlade(Vector2 bladeStart, Vector2 bladeEnd)
        {
            return bladeEnd.GeneratePointsAlong(50f, 5f, true, null, bladeStart)
                // .Select(p => p.Intify())
                .GetLineSegments()
                .ToList();
        }
        
        var start = generateBlade(between.Last().To, fanPoint);
        var end = generateBlade(fanPoint, between[0].From);
        var res = new List<LineSegment>(start);
        res.AddRange(between);
        res.AddRange(end);
        var stitch = res.OrderEndToStart(_data, poly);
        if (stitch.IsClockwise())
        {
            stitch = stitch.Select(ls => ls.GetReverse()).ToList();
        }
        
        
        if (stitch.IsCircuit() == false)
        {
            GD.Print("closing circuit");
            stitch = stitch.OrderEndToStart(_data, poly);
            if (stitch.IsCircuit() == false) throw new Exception();
        }
        
        return stitch;
    }
}
