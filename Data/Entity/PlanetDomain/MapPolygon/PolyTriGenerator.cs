
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyTriGenerator
{
    private Dictionary<MapPolygonBorder, int> _riverBorders;
    public void BuildTris(GenWriteKey key)
    {
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
            for (int i = 0; i < segs.Count; i++)
            {
                var seg = segs[i];
                var nSeg = nSegs[nSegs.Count - 1 - i];
                doPoint(seg.From, nSeg.To);
            }
            // doPoint(segs.First().From, nSegs.Last().To);
            // doPoint(segs.Last().To, nSegs.First().From);

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
        
        //todo new points not quite lining up
        var polys = key.Data.Planet.Polygons.Entities;
        var borders = key.Data.Planet.PolyBorders.Entities;
        
        var flowFloor = 50f;
        var flowCeiling = 10_000f;
        
        _riverBorders = borders.Where(b => b.MoistureFlow > flowFloor).ToDictionary(s => s, s => -1);
        
        var widthFloor = 10f;
        var widthCeil = 50f;
        var logBase = Mathf.Pow(flowCeiling, 1f / (widthCeil - widthFloor));
        
        var max = 0f;
        var min = Mathf.Inf;
        var avg = 0f;
        var bad = 0;

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
            if (numSegs > 2)
            {
                var index = Mathf.FloorToInt(numSegs / 2f);
                var middle = segs[index];
                var before = segs[index - 1];
                var after = segs[index + 1];
                var totLength = middle.Length() + before.Length() + after.Length();
                var axis = (after.To - before.From).Normalized();
                var spare = totLength - width;
                if (spare < 100f) throw new Exception();
                
                
                var p1 = (before.From + spare / 2f * axis).Intify();
                var p2 = (after.To - spare / 2f * axis).Intify();
                var newBefore = new LineSegment(before.From, p1);
                var rSeg = new LineSegment(p1, p2);
                var newAfter = new LineSegment(p2, after.To);
                
                var newSegs = new List<LineSegment>();
                
                for (int i = 0; i < index; i++)
                {
                    newSegs.Add(segs[i]);
                }
                newSegs.Add(newBefore);
                newSegs.Add(rSeg);
                var newIndex = index + 1;
                newSegs.Add(newAfter);
                for (int i = index + 1; i < segs.Count; i++)
                {
                    newSegs.Add(segs[i]);
                }
                rBorder.ReplacePoints(newSegs, 
                    key);
                if (newIndex == -1) throw new Exception();
                _riverBorders[rBorder] = newIndex;
            }
            else
            {
                var startP = segs.First().From;
                var endP = segs.Last().To;
                var totLength = startP.DistanceTo(endP);

                var axis = (endP - startP).Normalized();
                var spare = totLength - width;
                if (spare < 10f)
                {
                    var rSeg = new LineSegment(startP, endP);
                    var newSegs = new List<LineSegment> { rSeg };
                    rBorder.ReplacePoints(newSegs,
                        key);
                    _riverBorders[rBorder] = 0;
                }
                else
                {
                    var p1 = (startP + spare / 2f * axis).Intify();
                    var p2 = (endP - spare / 2f * axis).Intify();
                    var newBefore = new LineSegment(startP, p1);
                    var rSeg = new LineSegment(p1, p2);
                    var newAfter = new LineSegment(p2, endP);
                    var newSegs = new List<LineSegment> { newBefore, rSeg, newAfter };
                    rBorder.ReplacePoints(newSegs, 
                        key);
                    _riverBorders[rBorder] = 1;
                }
            }
        }
        GD.Print($"river borders {_riverBorders.Count()}");
        GD.Print($"bad {bad}");
        
        
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
            tris.Where(t => t.GetMinAltitude() > 5f).ToList(), 
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
        
        var tris = DelaunayTriangulator.TriangulatePoints(points)
            .Select(t =>
            {
                var land = lf.GetAtPoint(poly, t.GetCentroid(), key.Data);
                var veg = v.GetAtPoint(poly, t.GetCentroid(), land, key.Data);
                return new PolyTri(t.A, t.B, t.C, land, veg);
            })
            .ToList();
        var polyTerrainTris = PolyTerrainTris.Construct(
            tris.Where(t => t.GetMinAltitude() > 5f).ToList(), 
            key.Data);
        poly.SetTris(polyTerrainTris, key);
    }
    private void DoRiverPoly(MapPolygon poly, GenWriteKey key)
    {
        var lf = key.Data.Models.Landforms;
        var v = key.Data.Models.Vegetation;
        var tris = new List<PolyTri>();
        var borders = poly.Neighbors.Refs().Select(n => poly.GetBorder(n, key.Data)).ToList();
        var borderSegsRel = poly.GetAllBorderSegmentsClockwise(key.Data);
        var junctionPoints = new List<Vector2>();
        var riverSegs = borders.Where(_riverBorders.ContainsKey)
            .Select(br =>
            {
                var rIndex = _riverBorders[br];
                return br.HighId.Ref() == poly
                    ? br.HighSegsRel[rIndex]
                    : br.LowSegsRel[br.LowSegsRel.Count - 1 - rIndex];
            });
        var riverIndices = riverSegs.Select(rs => borderSegsRel.IndexOf(rs)).ToList();

        if (riverSegs.Count() < 2)
        {
            DoLandPolyNoRivers(poly, key);
            return;
        }
            
        for (var i = 0; i < riverIndices.Count; i++)
        {
            var riverIndex = riverIndices[i];
            var prevRiverIndex = riverIndices.Prev(i);
            var nextRiverIndex = riverIndices.Next(i);

            LineSegment rSeg = null;
            try
            {
                rSeg = borderSegsRel[riverIndex];
            }
            catch (Exception e)
            {
                throw new Exception(riverIndex.ToString());
            }
            var nextRSeg = borderSegsRel[nextRiverIndex];
            var prevRSeg = borderSegsRel[prevRiverIndex];

            
            var prevIntersect = GetIntersection(riverIndex, prevRiverIndex).Intify();
            var nextIntersect = GetIntersection(nextRiverIndex, riverIndex).Intify();

            junctionPoints.Add(nextIntersect);

            tris.Add(new PolyTri(rSeg.From, prevIntersect, nextIntersect, 
                LandformManager.River, VegetationManager.Barren));
            
            tris.Add(new PolyTri(rSeg.To, rSeg.From, nextIntersect,
                 LandformManager.River, VegetationManager.Barren));

            var thisNonRiverSegs = new List<LineSegment>();
            int iter = (riverIndex + 1) % borderSegsRel.Count;
            
            while (iter != nextRiverIndex)
            {
                thisNonRiverSegs.Add(borderSegsRel[iter]);
                iter++;
                iter = iter % borderSegsRel.Count;
            }

            
            var segTris = thisNonRiverSegs.TriangulateSegment(
                new LineSegment(nextIntersect, rSeg.To),
                new LineSegment(nextRSeg.From, nextIntersect)
                ).Select(t =>
            {
                var land = lf.GetAtPoint(poly, t.GetCentroid(), key.Data);
                var veg = v.GetAtPoint(poly, t.GetCentroid(), land, key.Data);
                return new PolyTri(t.A, t.B, t.C, land, veg);
            });

            tris.AddRange(segTris);
        }


        Vector2 GetIntersection(int fromIndex, int toIndex)
        {

            var to = borderSegsRel[fromIndex];
            var from = borderSegsRel[toIndex];
            if (to.Mid().Normalized() == -from.Mid().Normalized())
            {
                if (from.Mid() == Vector2.Zero) throw new Exception();
                if (to.Mid() == Vector2.Zero) throw new Exception();
                return to.From.LinearInterpolate(from.To,
                    from.Mid().Length() / (from.Mid().Length() + to.Mid().Length()));
            }
            
            var has = Vector2Ext.GetLineIntersection(to.From, to.From - to.Mid(),
                from.To, from.To - from.Mid(), 
                out var intersect);
            if (Mathf.IsNaN(intersect.x) || Mathf.IsNaN(intersect.y)) throw new Exception();

            return intersect;
        }

        for (var i = 1; i < junctionPoints.Count - 1; i++)
        {
            tris.Add(new PolyTri(junctionPoints[0], junctionPoints[i], junctionPoints[i + 1],
                LandformManager.River, VegetationManager.Barren));
        }
        var polyTerrainTris = PolyTerrainTris.Construct(
            tris.ToList(), 
            key.Data);
        poly.SetTris(polyTerrainTris, key);
    }
}
