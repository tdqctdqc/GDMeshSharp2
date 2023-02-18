using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Poly2Tri;
using Poly2Tri.Triangulation.Polygon;

public class PolyTriGenerator
{
    private Dictionary<MapPolygonBorder, int> _riverBorders;
    private Data _data;
    public void BuildTris(GenWriteKey key)
    {
        _data = key.Data;
        var polys = key.Data.Planet.Polygons.Entities;
        FindAndSizeRiverSegs(key);

        var max = 0f;
        var avg = 0f;
        var iter = 0;
        foreach (var p in polys)
        {
            iter++;
            var res = Check(p, key);
            max = Mathf.Max(max, res.max);
            avg += res.avg;
            BuildTris(p, key);
        }
        GD.Print("max deviation " + max);
        GD.Print("avg deviation " + (avg / iter));
    }

    private (float max, float avg) Check(MapPolygon p, WriteKey key)
    {      
        var max = 0f;
        var avg = 0f;
        var iter = 0;
        foreach (var n in p.Neighbors.Refs())
        {
            if (p.Center.DistanceTo(n.Center) > 1000f)
            {
                continue;
            }
            iter++;
            var offset = p.GetOffsetTo(n, key.Data);
            
            var b = p.GetBorder(n, key.Data);
            var segs = b.GetSegsRel(p);
            var nSegs = b.GetSegsRel(n);
            if (segs.Count == 0) continue;
            if (nSegs.Count != segs.Count) throw new Exception();
            for (int i = 0; i < segs.Count; i++)
            {
                var seg = segs[i];
                var nSeg = nSegs.FromEnd(i);
                doPoint(seg.From, nSeg.To);
            }
            void doPoint(Vector2 newSource, Vector2 oldCompare)
            {
                var p2 = newSource - offset;
                var dev = oldCompare.DistanceTo(p2);
                
                if (dev > 10f)
                {
                    GD.Print($"dev of {dev} at {p.Center} and {n.Center}");
                }
                max = Mathf.Max(max, dev);
                avg += dev;
            }
        }
        return (max, avg / iter);
    }
    
    
    private void FindAndSizeRiverSegs(GenWriteKey key)
    {
        var polys = key.Data.Planet.Polygons.Entities;
        var borders = key.Data.Planet.PolyBorders.Entities;
        
        var flowFloor = 50f;
        var flowCeiling = 10_000f;
        
        _riverBorders = borders.Where(b => b.MoistureFlow > flowFloor).ToDictionary(s => s, s => -1);
        
        var widthFloor = 5f;
        var widthCeil = 20f;
        var logBase = Mathf.Pow(flowCeiling, 1f / (widthCeil - widthFloor));
        
        var max = 0f;
        var min = Mathf.Inf;
        var avg = 0f;

        var rBorders = _riverBorders.Keys.ToList();
        foreach (var rBorder in rBorders)
        {
            var flow = rBorder.MoistureFlow;
            if (flow > flowCeiling) throw new Exception("river too wide for ceiling");
            var width = widthFloor + (float)Math.Log(flow, logBase);
            max = Mathf.Max(max, width);
            min = Mathf.Min(min, width);
            avg += width;

            var segs = rBorder.GetSegsAbs();
            var numSegs = segs.Count;
            var stitch0 = rBorder.HighSegsRel.StitchTogether();

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
            newSegs = newSegs.StitchTogether();
            
            var newIndex = preSegs.Count();
            rBorder.ReplacePoints(newSegs, 
                key);
            _riverBorders[rBorder] = newIndex;
            rBorder.SetRiverIndexHi(newIndex, key);
        }
        GD.Print($"river borders {_riverBorders.Count()}");
        
        
        GD.Print($"min {min} max {max} avg {avg / _riverBorders.Count()}");
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
        var borderSegsRel = poly.GetAllBorderSegmentsClockwise(key.Data);
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
        var borderSegsRel = poly.GetAllBorderSegmentsClockwise(key.Data);
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
        var borders = poly.Neighbors.Refs().Select(n => poly.GetBorder(n, key.Data)).ToList();
        var riverSegs = borders
            .Where(b => _riverBorders.ContainsKey(b))
            .Select(b => b.GetRiverSegment(poly)).ToList();
        riverSegs.OrderByClockwise(Vector2.Zero, ls => ls.From);
        var borderSegsRel = poly.GetAllBorderSegmentsClockwise(key.Data);
        var firstRSegIndex = borderSegsRel.IndexOf(riverSegs.First());
        var firstRSeg = riverSegs.First();
        if (riverSegs.Count == 1)
        {
            DoRiverSource(poly, borderSegsRel, firstRSeg, firstRSegIndex, tris, key);
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

    private void DoRiverSource(MapPolygon poly, List<LineSegment> borderSegs, LineSegment rSeg, 
        int rSegIndex, List<PolyTri> tris, GenWriteKey key)
    {
        var between = Enumerable.Range(1, borderSegs.Count - 1).Select(i => borderSegs.Modulo(rSegIndex + i)).ToList();
        if (between.Count != borderSegs.Count - 1) throw new Exception();
        if (between.Contains(rSeg)) throw new Exception();
        
        // var fan = BuildFan(rSeg, rSeg, Vector2.Zero, between);
        // TriangulateFan(poly, fan, tris);
        tris.Add(new PolyTri(rSeg.From, rSeg.To, Vector2.Zero, LandformManager.River, VegetationManager.Barren));

        try
        {
            var outline = GetOutline(Vector2.Zero, between);
            TriangulateArbitrary(poly, outline, tris, key);
        }
        catch (Exception e)
        {
            GD.Print($"Bad r source triangulation at {poly.Center} {poly.Id}");
        }
        
    }

    private void DoRiverJunction(MapPolygon poly, List<LineSegment> borderSegsRel, 
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
                junctionPoints.Add(nextIntersect
                    // .Intify() 
                    );
            }
            else
            {
                tris.Add(new PolyTri(nextIntersect, Vector2.Zero, rSeg.From,
                    LandformManager.River, VegetationManager.Barren));
                tris.Add(new PolyTri(rSeg.To, nextIntersect, rSeg.From,
                    LandformManager.River, VegetationManager.Barren));
                tris.Add(new PolyTri(nextRSeg.From, nextIntersect, Vector2.Zero,
                    LandformManager.River, VegetationManager.Barren));
                junctionPoints.Add(nextIntersect
                    // .Intify()
                );
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

                try
                {
                    var outline = GetOutline(intersect, between);
                    TriangulateArbitrary(poly, outline, tris, key);
                }
                catch (Exception e)
                {
                    GD.Print($"Bad triangulation at {poly.Center} {poly.Id}");
                    continue;
                }
                
                
                // between.ForEach(s =>
                // {
                //     tris.Add(new PolyTri(s.From, s.To, intersect, 
                //         LandformManager.Plain, VegetationManager.Barren));
                // });
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
        var points = 
            new List<Vector2>();
        // outline.GenerateInteriorPoints(50f, 10f);
        points.AddRange(outline.GetPoints());
        var polygon = new Polygon(points.Select(v => new PolygonPoint(v.x, v.y)));
        Poly2Tri.P2T.Triangulate(polygon);
                    
        tris.AddRange(
            polygon.Triangles.Select(dt =>
            {
                var c = new Vector2(dt.Centroid().Xf, dt.Centroid().Yf);
                var l = lf.GetAtPoint(poly, c, key.Data);
                var v = veg.GetAtPoint(poly, c, l, key.Data);
                return new PolyTri(
                    new Vector2(dt.Points.Item0.Xf, dt.Points.Item0.Yf),
                    new Vector2(dt.Points.Item1.Xf, dt.Points.Item1.Yf),
                    new Vector2(dt.Points.Item2.Xf, dt.Points.Item2.Yf),
                    l, v
                );
            })
        );
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

    private List<LineSegment> GetOutline(Vector2 fanPoint, List<LineSegment> between)
    {
        List<LineSegment> generateBlade(Vector2 bladeStart, Vector2 bladeEnd)
        {
            var blade = bladeEnd.GeneratePointsAlong(50f, 5f, true, null, bladeStart)
                // .Select(p => p.Intify())
                .GetLineSegments()
                .ToList();
            
            return blade.StitchTogether();
        }

        var start = generateBlade(fanPoint, between[0].From);
        var end = generateBlade(between.Last().To, fanPoint);
        var edge = between.StitchTogether();
        

        var res = new List<LineSegment>(start);
        res.AddRange(edge);
        res.AddRange(end);
        try
        {
            var stitch = res.StitchTogether();

        }
        catch (Exception e)
        {
            // GD.Print("START");
            // for (var i = 0; i < start.Count; i++)
            // {
            //     GD.Print($"start from {start[i].From} to {start[i].To}");
            // }
            //
            // GD.Print("BETWEEN");
            // for (var i = 0; i < edge.Count; i++)
            // {
            //     GD.Print($"edge from {edge[i].From} to {edge[i].To}");
            // }
            //
            // GD.Print("END");
            // for (var i = 0; i < end.Count; i++)
            // {
            //     GD.Print($"end from {end[i].From} to {end[i].To}");
            // }

            throw;
        }
        return res;
    }
    private Fan BuildFan(Vector2 fanPoint, List<LineSegment> between)
    {
        var blades = new List<List<LineSegment>>();
        var edges = new List<List<LineSegment>>();
        var riverFrom = between[0].From;
        var riverTo = between.Last().To;
        List<LineSegment> generateBlade(Vector2 bladeStart, Vector2 bladeEnd)
        {
            var blade = bladeEnd.GeneratePointsAlong(30f, 5f, true, null, bladeStart)
                // .Select(p => p.Intify())
                .GetLineSegments()
                .ToList();
            
            return blade.StitchTogether();
        }

        var startRay = riverFrom - fanPoint;
        var startBlade = generateBlade(fanPoint, riverFrom);
        blades.Add(startBlade);

        var currEdge = new List<LineSegment>();
        
        for (var i = 0; i < between.Count; i++)
        {
            var seg = between[i];

            var ray = seg.To - fanPoint;

            if (Mathf.Abs(ray.AngleTo(startRay)) >= Mathf.Pi)
            {
                var newBlade = generateBlade(seg.To, fanPoint);
                blades.Add(newBlade);
                blades.Add(newBlade.GetOpposing(Vector2.Zero).ToList());
                edges.Add(currEdge);
                if (currEdge.Count == 0)
                {
                    GD.Print($"empty edge at {i} / {between.Count}");
                    GD.Print(ray.GetClockwiseAngleTo(startRay).RadToDegrees() + " degrees");
                    throw new Exception();
                }
                currEdge = new List<LineSegment> {seg};
                startRay = ray;
            }
            else
            {
                currEdge.Add(seg);
            }
        }

        var endBlade = generateBlade(riverTo, fanPoint);
        blades.Add(endBlade);
        edges.Add(currEdge);

        return new Fan(blades, edges);
    }

    private void TriangulateFan(MapPolygon poly, Fan fan, List<PolyTri> tris)
    {
        if (fan.Edges.Count * 2 != fan.Blades.Count) throw new Exception();
        for (var i = 0; i < fan.Edges.Count; i++)
        {
            var edge = fan.Edges[i];
            var startBlade = fan.Blades[2 * i];
            if (startBlade.Count == 0) throw new Exception();
            var endBlade = fan.Blades[2 * i + 1];
            if (endBlade.Count == 0) throw new Exception();

            
            var edgeSegs = fan.Edges[i];
            if (edgeSegs.Count == 0) throw new Exception();
            
            TriangulateConvexFanSegment(poly, startBlade, endBlade, edgeSegs, tris);
        }
    }

    private void TriangulateConvexFanSegment(MapPolygon poly, List<LineSegment> startBlade, List<LineSegment> endBlade,
        List<LineSegment> edgeSegments, List<PolyTri> tris)
    {
        var allSegs = startBlade.Union(endBlade).Union(edgeSegments).ToList();
        var first = allSegs[0];
        
        for (var i = 1; i < allSegs.Count; i++)
        {
            tris.Add(new PolyTri(first.From, allSegs[i].From, allSegs[i].To, 
                LandformManager.Plain, VegetationManager.Barren));
        }
        // var points = new HashSet<Vector2>(allSegs.GetPoints()).ToList();
        // points.AddRange(allSegs.GenerateInteriorPoints(30f, 5f));
        // tris.AddRange(
        //     DelaunayTriangulator.PolyTriangulatePoints(poly, points, _data)
        //         .Where(t => t.IsDegenerate() == false));
    }
}
