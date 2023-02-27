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
    private IDDispenser _id;
    public void BuildTris(GenWriteKey key, IDDispenser id)
    {
        _id = id;
        _data = key.Data;
        _riverBorders = new Dictionary<MapPolygonBorder, int>();
        var polys = _data.Planet.Polygons.Entities;
        FindAndSizeRiverSegs(key);
        foreach (var p in polys)
        {
            BuildTris(p, key);
        }
    }
    
    private void FindAndSizeRiverSegs(GenWriteKey key)
    {
        var polys = _data.Planet.Polygons.Entities;
        var borders = _data.Planet.PolyBorders.Entities;
        
        _riverBorders = borders.Where(b => b.MoistureFlow > River.FlowFloor).ToDictionary(s => s, s => -1);
        
        var logBase = Mathf.Pow(River.FlowCeil, 1f / (River.WidthCeil - River.WidthFloor));
        
        var max = 0f;
        var min = Mathf.Inf;
        var avg = 0f;

        foreach (var rBorder in _riverBorders.Keys.ToList())
        {
            var hi = rBorder.HighId.Entity();

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
            var p1 = (mid - axis * width / 2f);
            var preSegs = p1.GeneratePointsAlong(50f, 10f, true, null, start).GetLineSegments();

            var p2 = (mid + axis * width / 2f);
            var postSegs = end.GeneratePointsAlong(50f, 10f, true, null, p2).GetLineSegments();
            var rSeg = new LineSegment(p1, p2);
            var newSegs = preSegs.ToList();
            newSegs.Add(rSeg);
            newSegs.AddRange(postSegs);
            newSegs = newSegs.OrderEndToStart(_data, rBorder.HighId.Entity());
            
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
        else if (poly.Neighbors.Refs().Any(n => _riverBorders.ContainsKey(poly.GetBorder(n, _data))))
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
        var lf = _data.Models.Landforms;
        var v = _data.Models.Vegetation;
        var points = new List<Vector2> {Vector2.Zero};
        points.AddRange(borderSegsRel.GetPoints());
        
        var tris = DelaunayTriangulator.TriangulatePoints(points)
            .Select(t =>
            {
                return new PolyTri(_id.GetID(), t.A, t.B, t.C, LandformManager.Sea, VegetationManager.Barren);
            })
            .ToList();
        var polyTerrainTris = PolyTerrainTris.Construct(poly,
            tris.ToList(), 
            _data);
        poly.SetTris(polyTerrainTris, key);
    }
    
    private void DoLandPolyNoRivers(MapPolygon poly, GenWriteKey key)
    {
        var borderSegs = poly.BorderSegments;
        var lf = _data.Models.Landforms;
        var v = _data.Models.Vegetation;
        var points = borderSegs.GenerateInteriorPoints(50f, 10f)
            .ToList();
        points.AddRange(borderSegs.GetPoints());


        var tris  = DelaunayTriangulator.TriangulatePoints(points)
            .Select(t =>
            {
                var tlf = lf.GetAtPoint(poly, t.GetCentroid(), _data);
                var tv = v.GetAtPoint(poly, t.GetCentroid(), tlf, _data);
                return new PolyTri(_id.GetID(), t.A, t.B, t.C, tlf, tv);
            })
            .ToList();

        var polyTerrainTris = PolyTerrainTris.Construct(poly,
            tris, 
            _data);
        poly.SetTris(polyTerrainTris, key);
    }
    private void DoRiverPoly(MapPolygon poly, GenWriteKey key)
    {
        var lf = _data.Models.Landforms;
        var v = _data.Models.Vegetation;
        var tris = new List<PolyTri>();
        var borders = poly.Neighbors.Refs().Select(n => poly.GetBorder(n, _data));
        var rBorders = borders
            .Where(b => _riverBorders.ContainsKey(b)).ToList();
        var riverSegs = rBorders
            .Select(b => b.GetRiverSegment(poly)).ToList();
        riverSegs.OrderByClockwise(Vector2.Zero, ls => ls.From);
        
        if (riverSegs.Count == 1)
        {
            DoRiverSource(poly, rBorders.First(), tris, key);
        }
        else
        {
            DoRiverJunction(poly, rBorders, tris, key);
        }
        var polyTerrainTris = PolyTerrainTris.Construct(poly,
            tris.ToList(), 
            _data);
        poly.SetTris(polyTerrainTris, key);
    }

    private void DoRiverSource(MapPolygon poly, MapPolygonBorder rBorder, 
        List<PolyTri> tris, GenWriteKey key)
    {
        var borderSegs = poly.BorderSegments;
        var rSeg = borderSegs.First(s => s.Mid().GetClockwiseAngle() == rBorder.GetRiverSegment(poly).Mid().GetClockwiseAngle());
        
        
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

        var rTri = new PolyTri(_id.GetID(), rSeg.From, rSeg.To, Vector2.Zero, LandformManager.River, VegetationManager.Barren);
        tris.Add(rTri);
        var outline = GetOutline(poly, Vector2.Zero, between);
        TriangulateArbitrary(poly, outline, tris, key);
    }

    private void DoRiverJunction(MapPolygon poly, List<MapPolygonBorder> riverBorders,
        List<PolyTri> tris, GenWriteKey key)
    {
        var borderSegs = poly.BorderSegments;
        var avg = borderSegs.Average();
        var riverSegs = riverBorders.Select(b => b.GetRiverSegment(poly))
            .Select(rs => 
                borderSegs
                        .OrderBy(s => Mathf.Abs(s.Mid().GetClockwiseAngle() - rs.Mid().GetClockwiseAngle()))
                .First()
                )
            .ToList();
        riverSegs.OrderByClockwise(Vector2.Zero, ls => ls.From);
        var firstRSeg = riverSegs.First();
        var firstRSegIndex = borderSegs.IndexOf(firstRSeg);

        var junctionPoints = new List<Vector2>();
        var lf = _data.Models.Landforms;
        var veg = _data.Models.Vegetation;
        
        for (var i = 0; i < riverSegs.Count; i++)
        {
            var rSeg = riverSegs[i];
            var nextRSeg = riverSegs.Next(i);
            var lAvg = (rSeg.Length() + nextRSeg.Length()) / 2f;

            var angle = rSeg.To.GetClockwiseAngleTo(nextRSeg.From) / 2f;
            var nextIntersect = (rSeg.To.Rotated(angle).Normalized() * lAvg);
            
            junctionPoints.Add(nextIntersect);

            if (riverSegs.Count == 2)
            {
                tris.Add(new PolyTri(_id.GetID(), rSeg.From, rSeg.To, nextIntersect,
                    LandformManager.River, VegetationManager.Barren));
                tris.Add(new PolyTri(_id.GetID(), rSeg.From, nextIntersect, -nextIntersect,
                    LandformManager.River, VegetationManager.Barren));
            }
            else
            {
                tris.Add(new PolyTri(_id.GetID(), rSeg.From, rSeg.To, Vector2.Zero,
                    LandformManager.River, VegetationManager.Barren));
                
                tris.Add(new PolyTri(_id.GetID(), -nextIntersect, Vector2.Zero, nextRSeg.To, 
                    LandformManager.River, VegetationManager.Barren));
                
                tris.Add(new PolyTri(_id.GetID(), rSeg.From, -nextIntersect, Vector2.Zero,
                    LandformManager.River, VegetationManager.Barren));
            }
        }
        
        for (var i = 0; i < riverSegs.Count; i++)
        {
            var rSeg = riverSegs[i];
            var nextRSeg = riverSegs.Next(i);
            var between = borderSegs.GetBetween(nextRSeg, rSeg);
            var intersect = -junctionPoints[i];
            
            var outline = GetOutline(poly, intersect, between);
            TriangulateArbitrary(poly, outline, tris, key);
        }
    }

    private void TriangulateArbitrary(MapPolygon poly, IReadOnlyList<LineSegment> outline, List<PolyTri> tris, GenWriteKey key)
    {
        var lf = _data.Models.Landforms;
        var veg = _data.Models.Vegetation;

        var interior = outline.GenerateInteriorPoints(30f, 10f).ToHashSet();
        tris.AddRange(outline.PolyTriangulate(key.GenData, poly, _id, interior));
    }

    private List<LineSegment> GetOutline(MapPolygon poly, Vector2 fanPoint, List<LineSegment> between)
    {
        List<LineSegment> generateBlade(Vector2 bladeStart, Vector2 bladeEnd)
        {
            return bladeEnd.GeneratePointsAlong(50f, 5f, true, null, bladeStart)
                .GetLineSegments()
                .ToList();
        }
        
        var start = generateBlade(between.Last().To, fanPoint);
        var end = generateBlade(fanPoint, between[0].From);
        var res = new List<LineSegment>();
        res.AddRange(between);
        res.AddRange(start);
        res.AddRange(end);

        if (res.IsCircuit() == false)
            throw new SegmentsNotConnectedException(_data, poly, between, res,
                start, end);
        
        return res;
    }
}
