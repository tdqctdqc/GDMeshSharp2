using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Poly2Tri;
using Poly2Tri.Triangulation.Polygon;
using Poly2Tri.Triangulation.Sets;
using Poly2Tri.Utility;

public class PolyTriGenerator : Generator
{
    private ConcurrentDictionary<MapPolygonEdge, int> _riverBorders;
    private GenData _data;
    private IdDispenser _idd;

    public PolyTriGenerator()
    {
    }
    public override GenReport Generate(GenWriteKey key)
    {
        _idd = key.IdDispenser;
        _data = key.GenData;
        var report = new GenReport(GetType().Name);
        
        _riverBorders = new ConcurrentDictionary<MapPolygonEdge, int>();
        var polys = _data.Planet.Polygons.Entities;
        
        report.StartSection();
        FindAndSizeRiverSegs(key);
        report.StopSection("Finding river segs");
        
        report.StartSection();
        Parallel.ForEach(polys, p => BuildTris(p, key));
        report.StopSection("Building poly terrain tris");
        return report;
    }
    
    private void FindAndSizeRiverSegs(GenWriteKey key)
    {
        var polys = _data.Planet.Polygons.Entities;
        var borders = _data.Planet.PolyEdges.Entities;
        var prefEdgeLength = _data.GenMultiSettings.PlanetSettings.PreferredMinPolyEdgeLength.Value;
        
        var dic = borders.Where(b => b.MoistureFlow > River.FlowFloor).ToDictionary(s => s, s => -1);
        _riverBorders = new ConcurrentDictionary<MapPolygonEdge, int>(dic);
        
        var logBase = Mathf.Pow(River.FlowCeil, 1f / (River.WidthCeil - River.WidthFloor));
        
        var max = 0f;
        var min = Mathf.Inf;
        var avg = 0f;
        Parallel.ForEach(_riverBorders.Keys, doEdge);
        _data.Notices.SetPolyShapes.Invoke();
        return;
            
        void doEdge(MapPolygonEdge rBorder)
        {
            var hi = rBorder.HighId.Entity();

            var flow = rBorder.MoistureFlow;
            if (flow > River.FlowCeil)
            {
                GD.Print($"flow is {flow} too wide for ceiling {River.FlowCeil}");
                flow = River.FlowCeil;
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
            var preSegs = p1.GeneratePointsAlong(prefEdgeLength, prefEdgeLength / 5f, true, null, start).GetLineSegments();

            var p2 = (mid + axis * width / 2f);
            var postSegs = end.GeneratePointsAlong(prefEdgeLength, prefEdgeLength / 5f, true, null, p2).GetLineSegments();
            var rSeg = new LineSegment(p1, p2);
            var newSegs = preSegs.ToList();
            newSegs.Add(rSeg);
            newSegs.AddRange(postSegs);
            newSegs = newSegs.Ordered<LineSegment, Vector2>();
            
            if(newSegs.IsContinuous() == false)
            {
                var ex = new SegmentsException();
                ex.AddSegLayer(hi.GetOrderedBoundarySegs(_data).ToList(), "poly boundary");
                ex.AddSegLayer(newSegs, "new segs");
                throw ex;
            }
            
            var newIndex = preSegs.Count();
            rBorder.ReplacePoints(newSegs, newIndex,
                key);
            _riverBorders[rBorder] = newIndex;
        }
    }

    private void BuildTris(MapPolygon poly, GenWriteKey key)
    {
        List<PolyTri> tris;
        var graph = new Graph<PolyTri, bool>();
        if (poly.IsWater())
        {
            tris = DoSeaPoly(poly, key);
        }
        // else if (poly.Neighbors.Entities().Any(n => _riverBorders.ContainsKey(poly.GetEdge(n, _data))))
        // {
        //     tris = DoRiverPoly(poly, key);
        // }
        else
        {
            tris = DoLandPolyNoRivers(poly, key);
        }
            
        var polyTerrainTris = PolyTris.Create(tris,  null, key);
        poly.SetTerrainTris(polyTerrainTris, key);
    }
    
    private List<PolyTri> DoSeaPoly(MapPolygon poly, GenWriteKey key)
    {
        var borderSegsRel = poly.GetOrderedBoundarySegs(key.Data);
        var lf = _data.Models.Landforms;
        var v = _data.Models.Vegetation;
        var points = borderSegsRel.GetPoints().ToList();
        
            
            
            
        points.Add(Vector2.Zero);
        
        var tris = DelaunayTriangulator.TriangulatePoints(points)
            .Select(t =>
            {
                return PolyTri.Construct(t.A, t.B, t.C, 
                    LandformManager.Sea.MakeRef(), 
                    VegetationManager.Barren.MakeRef());
            })
            .ToList();
        return tris;
    }
    
    private List<PolyTri> DoLandPolyNoRivers(MapPolygon poly, GenWriteKey key)
    {
        var borderSegs = poly.GetOrderedBoundarySegs(key.Data);
        var points = borderSegs.GetPoints().ToList();
        var graph = new Graph<PolyTri, bool>();

        List<PolyTri> tris;
        int iter = 0;

        while (true)
        {
            var innerPoints = borderSegs.GenerateInteriorPoints(50f, 10f);
            innerPoints.AddRange(points);
            try
            {
                iter++;
                tris = DelaunayTriangulator.TriangulatePointsAndGetTriAdjacencies<PolyTri>
                (
                    points,
                    graph,
                    (a, b, c) =>
                    {
                        var lf = key.Data.Models.Landforms.GetAtPoint(poly, (a + b + c) / 3f, key.Data);
                        var v = key.Data.Models.Vegetation.GetAtPoint(poly, (a + b + c) / 3f, lf, key.Data);
                        return PolyTri.Construct(a, b, c, lf.MakeRef(), v.MakeRef());
                    }
                );
                break;
            }
            catch
            {
                if (iter > 100) throw;
                GD.Print($"triangulation for poly {poly.Id} failed, trying");
            }
        }
        
        // points.AddRange(borderSegs.GenerateInteriorPoints(50f, 10f));
        

        return tris;
    }
    private List<PolyTri> DoRiverPoly(MapPolygon poly, GenWriteKey key)
    {
        var lf = _data.Models.Landforms;
        var v = _data.Models.Vegetation;
        List<PolyTri> tris;
        var rBorders = poly.GetPolyBorders()
            .Where(b => b.HasRiver()).ToList();
        
        if (rBorders.Count == 1)
        {
            tris = DoRiverSource(poly, rBorders.First(), key);
        }
        else
        {
            tris = DoRiverJunction(poly, rBorders, key);
        }
        return tris;
    }

    private List<PolyTri> DoRiverSource(MapPolygon poly, PolyBorderChain rEdge, GenWriteKey key)
    {
        var tris = new List<PolyTri>();
        var boundarySegs = poly.GetOrderedBoundarySegs(key.Data);
        var rSeg = boundarySegs
            .FirstOrDefault(s => Mathf.Abs(s.Mid().Angle() 
                                  - rEdge.GetRiverSegment().Mid().Angle()) < .01f);
        if (rSeg == null)
        {
            //todo fix this
            return DoLandPolyNoRivers(poly, key);
        }
        
        var rSegIndex = boundarySegs.IndexOf(rSeg);
        var between = Enumerable.Range(1, boundarySegs.Count - 1)
            .Select(i => boundarySegs.Modulo(rSegIndex + i))
            .ToList();

        var rTri = PolyTri.Construct(rSeg.From, rSeg.To, Vector2.Zero, LandformManager.River.MakeRef(), 
            VegetationManager.Barren.MakeRef());
        tris.Add(rTri);
        var outline = GetOutline(poly, Vector2.Zero, between);
        var count1 = outline.Count / 2;
        var count2 = outline.Count - count1;
        
        var o1 = outline.GetRange(0, count1);
        o1.Add(new LineSegment(o1.Last().To, o1.First().From));
        
        var o2 = outline.GetRange(count1, count2);
        o2.Add(new LineSegment(o2.Last().To, o2.First().From));

        tris.AddRange(TriangulateArbitrary(poly, o1, key, true));
        tris.AddRange(TriangulateArbitrary(poly, o2, key, true));
        return tris;
    }

    private List<PolyTri> DoRiverJunction(MapPolygon poly, List<PolyBorderChain> riverBorders,
        GenWriteKey key)
    {
        var borderSegs = poly.GetOrderedBoundarySegs(key.Data);
        var tris = new List<PolyTri>();
        var riverSegs = riverBorders.Select(b => b.GetRiverSegment())
            .ToList();
        foreach (var r in riverBorders)
        {
            if(borderSegs.Contains(r.GetRiverSegment()) == false)
            {
                GD.Print("river index " + r.RiverSegmentIndex);
                //todo fix back
                return DoLandPolyNoRivers(poly, key);
                // throw new Exception();
            }
        }
        if (riverSegs.Any(rs => borderSegs.Contains(rs) == false))
        {
        }
        riverSegs.OrderByClockwise(Vector2.Zero, ls => ls.From);

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
                tris.Add(PolyTri.Construct(rSeg.From, rSeg.To, nextIntersect,
                    LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
                tris.Add(PolyTri.Construct(rSeg.From, nextIntersect, -nextIntersect,
                    LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
            }
            else
            {
                tris.Add(PolyTri.Construct(rSeg.From, rSeg.To, Vector2.Zero,
                    LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
                
                tris.Add(PolyTri.Construct(-nextIntersect, Vector2.Zero, nextRSeg.To, 
                    LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
                
                tris.Add(PolyTri.Construct(rSeg.From, -nextIntersect, Vector2.Zero,
                    LandformManager.River.MakeRef(), VegetationManager.Barren.MakeRef()));
            }
        }
        
        for (var i = 0; i < riverSegs.Count; i++)
        {
            var rSeg = riverSegs[i];
            var nextRSeg = riverSegs.Next(i);
            var between = borderSegs.GetBetween(nextRSeg, rSeg);
            var intersect = -junctionPoints[i];
            
            var outline = GetOutline(poly, intersect, between);
            tris.AddRange(TriangulateArbitrary(poly, outline, key, true));
        }

        return tris;
    }

    private List<PolyTri> TriangulateArbitrary(MapPolygon poly, IReadOnlyList<LineSegment> outline, 
        GenWriteKey key, bool generateInterior)
    {
        HashSet<Vector2> interior = generateInterior 
            ? outline.GenerateInteriorPoints(30f, 10f).ToHashSet()
            : null;
        return outline.PolyTriangulate(key.GenData, poly, _idd, interior);
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
        {
            var ex = new SegmentsException();
            ex.AddSegLayer(between, "between");
            ex.AddSegLayer(res, "res");
            ex.AddSegLayer(start, "start");
            ex.AddSegLayer(end, "end");
            throw ex;
        }
        
        return res;
    }
}
