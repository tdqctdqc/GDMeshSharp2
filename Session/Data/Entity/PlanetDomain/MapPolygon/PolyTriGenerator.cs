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
    private Dictionary<MapPolygonEdge, int> _riverBorders;
    private GenData _data;
    private IdDispenser _idd;
    public void BuildTris(GenWriteKey key, IdDispenser id)
    {
        _idd = id;
        _data = key.GenData;
        _riverBorders = new Dictionary<MapPolygonEdge, int>();
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
        var borders = _data.Planet.PolyEdges.Entities;
        
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

            var segs = rBorder.GetSegsAbs();
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
            newSegs = newSegs.OrderEndToStart();
            
            if(newSegs.IsContinuous() == false)
            {
                throw new SegmentsNotConnectedException(key.GenData, hi, hi.GetBoundarySegments(_data).ToList(), newSegs, null);
            }
            
            var newIndex = preSegs.Count();
            rBorder.ReplacePoints(newSegs, 
                key);
            _riverBorders[rBorder] = newIndex;
            rBorder.SetRiverIndexHi(newIndex, key);
        }
        _data.Events.SetPolyShapes?.Invoke();
    }

    private void BuildTris(MapPolygon poly, GenWriteKey key)
    {
        if (poly.IsWater())
        {
            DoSeaPoly(poly, key);
        }
        else if (poly.Neighbors.Refs().Any(n => _riverBorders.ContainsKey(poly.GetEdge(n, _data))))
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
        var borderSegsRel = poly.GetBoundarySegments(key.Data);
        var lf = _data.Models.Landforms;
        var v = _data.Models.Vegetation;
        var points = new List<Vector2> {Vector2.Zero};
        points.AddRange(borderSegsRel.GetPoints());
        
        var tris = DelaunayTriangulator.TriangulatePoints(points)
            .Select(t =>
            {
                return new PolyTri(_idd.GetID(), t.A, t.B, t.C, LandformManager.Sea.GetRef(), 
                    VegetationManager.Barren.GetRef());
            })
            .ToList();
        var polyTerrainTris = PolyTerrainTris.Create(poly, tris.ToList(), key);
    }
    
    private void DoLandPolyNoRivers(MapPolygon poly, GenWriteKey key)
    {
        var borderSegs = poly.GetBoundarySegments(key.Data);
        var points = borderSegs.GenerateInteriorPoints(50f, 10f)
            .ToHashSet();
        var tris = borderSegs.PolyTriangulate(key.GenData, poly, _idd, points);

        var polyTerrainTris = PolyTerrainTris.Create(poly, tris, key);
    }
    private void DoRiverPoly(MapPolygon poly, GenWriteKey key)
    {
        var lf = _data.Models.Landforms;
        var v = _data.Models.Vegetation;
        var tris = new List<PolyTri>();
        var borders = poly.Neighbors.Refs().Select(n => poly.GetEdge(n, _data));
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
        var polyTerrainTris = PolyTerrainTris.Create(poly,
            tris.ToList(), key);
    }

    private void DoRiverSource(MapPolygon poly, MapPolygonEdge rEdge, 
        List<PolyTri> tris, GenWriteKey key)
    {
        var boundarySegs = poly.GetBoundarySegments(key.Data);
        var rSeg = boundarySegs
            .First(s => Mathf.Abs(s.Mid().Angle() 
                                  - rEdge.GetRiverSegment(poly).Mid().Angle()) < .01f);
        
        var rSegIndex = boundarySegs.IndexOf(rSeg);
        var between = Enumerable.Range(1, boundarySegs.Count - 1)
            .Select(i => boundarySegs.Modulo(rSegIndex + i))
            .ToList();

        var rTri = new PolyTri(_idd.GetID(), rSeg.From, rSeg.To, Vector2.Zero, LandformManager.River.GetRef(), 
            VegetationManager.Barren.GetRef());
        tris.Add(rTri);
        var outline = GetOutline(poly, Vector2.Zero, between);
        var count1 = outline.Count / 2;
        var count2 = outline.Count - count1;
        
        var o1 = outline.GetRange(0, count1);
        o1.Add(new LineSegment(o1.Last().To, o1.First().From));
        
        var o2 = outline.GetRange(count1, count2);
        o2.Add(new LineSegment(o2.Last().To, o2.First().From));

        TriangulateArbitrary(poly, o1, tris, key);
        TriangulateArbitrary(poly, o2, tris, key);
    }

    private void DoRiverJunction(MapPolygon poly, List<MapPolygonEdge> riverBorders,
        List<PolyTri> tris, GenWriteKey key)
    {
        var borderSegs = poly.GetBoundarySegments(key.Data);;
        var avg = borderSegs.Average();
        var riverSegs = riverBorders.Select(b => b.GetRiverSegment(poly))
            .Select(rs => 
                borderSegs
                        .OrderBy(s => Mathf.Abs(s.Mid().GetCCWAngle() - rs.Mid().GetCCWAngle()))
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

            var angle = rSeg.To.GetCCWAngleTo(nextRSeg.From) / 2f;
            var nextIntersect = (rSeg.To.Rotated(angle).Normalized() * lAvg);
            
            junctionPoints.Add(nextIntersect);

            if (riverSegs.Count == 2)
            {
                tris.Add(new PolyTri(_idd.GetID(), rSeg.From, rSeg.To, nextIntersect,
                    LandformManager.River.GetRef(), VegetationManager.Barren.GetRef()));
                tris.Add(new PolyTri(_idd.GetID(), rSeg.From, nextIntersect, -nextIntersect,
                    LandformManager.River.GetRef(), VegetationManager.Barren.GetRef()));
            }
            else
            {
                tris.Add(new PolyTri(_idd.GetID(), rSeg.From, rSeg.To, Vector2.Zero,
                    LandformManager.River.GetRef(), VegetationManager.Barren.GetRef()));
                
                tris.Add(new PolyTri(_idd.GetID(), -nextIntersect, Vector2.Zero, nextRSeg.To, 
                    LandformManager.River.GetRef(), VegetationManager.Barren.GetRef()));
                
                tris.Add(new PolyTri(_idd.GetID(), rSeg.From, -nextIntersect, Vector2.Zero,
                    LandformManager.River.GetRef(), VegetationManager.Barren.GetRef()));
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

    private void TriangulateArbitrary(MapPolygon poly, IReadOnlyList<LineSegment> outline, 
        List<PolyTri> tris, GenWriteKey key)
    {
        var interior = outline.GenerateInteriorPoints(30f, 10f).ToHashSet();
        tris.AddRange(outline.PolyTriangulate(key.GenData, poly, _idd, interior));
    }

    private List<LineSegment> GetOutline(MapPolygon poly, Vector2 fanPoint, List<LineSegment> between)
    {
        List<LineSegment> generateBlade(Vector2 bladeStart, Vector2 bladeEnd)
        {
            return bladeEnd.GeneratePointsAlong(50f, 5f, true, null, bladeStart)
                .GetLineSegments()
                .ToList();
        }
        
        var end = generateBlade(between.Last().To, fanPoint);
        var start = generateBlade(fanPoint, between[0].From);
        var res = new List<LineSegment>();
        res.AddRange(start);
        res.AddRange(between);
        res.AddRange(end);

        if (res.IsCircuit() == false)
            throw new SegmentsNotConnectedException(_data, poly, between, res,
                start, end);
        
        return res;
    }
}
