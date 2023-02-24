using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Poly2Tri.Triangulation;
using Poly2Tri.Triangulation.Sets;
using Poly2Tri.Utility;

public static class LineSegmentExt
{
    public static IEnumerable<LineSegment> GetOpposing(this IEnumerable<LineSegment> segs, Vector2 offset)
    {
        return segs.Select(s => s.Translate(-offset).GetReverse()).Reverse();
    }

    public static IEnumerable<LineSegment> ChangeOrigin(this IEnumerable<LineSegment> segs, Vector2 oldOrigin,
        Vector2 newOrigin)
    {
        return segs.Select(l => l.ChangeOrigin(oldOrigin, newOrigin));
    }

    public static IEnumerable<LineSegment> ChangeOrigin(this IEnumerable<LineSegment> segs,
        MapPolygon oldOrigin, MapPolygon newOrigin, Data data)
    {
        var offset = newOrigin.GetOffsetTo(oldOrigin, data);
        return segs.Select(l => new LineSegment(l.From + offset,
            l.To + offset));
    }

    public static void SplitToMinLength(this MapPolygonBorder border, float minLength, GenWriteKey key)
    {
        var newSegsAbs = new List<LineSegment>();
        var segs = border.GetSegsAbs();
        var offset = border.HighId.Ref().GetOffsetTo(border.LowId.Ref(), key.Data);
        for (var i = 0; i < segs.Count; i++)
        {
            var seg = segs[i];
            var axis = (seg.To - seg.From);
            var l = seg.Length();
            if (l > minLength * 2f)
            {
                var numSplits = Mathf.FloorToInt(l / minLength) - 1;
                var prev = seg.From;
                for (int j = 1; j <= numSplits; j++)
                {
                    var interp = j / (numSplits + 1f);
                    var splitP = (seg.From + axis * interp).Intify();

                    newSegsAbs.Add(new LineSegment(prev, splitP));
                    prev = splitP;
                }

                newSegsAbs.Add(new LineSegment(prev, seg.To));
            }
            else
            {
                newSegsAbs.Add(seg);
            }
        }

        border.ReplacePoints(newSegsAbs,
            key);
    }

    public static bool IsCircuit(this List<LineSegment> segs)
    {
        for (int i = 0; i < segs.Count - 1; i++)
        {
            if (segs[i].To != segs[i + 1].From) return false;
        }
        if (segs[segs.Count - 1].To != segs[0].From) return false;

        return true;
    }

    public static bool IsConvexAround(this List<LineSegment> segs, Vector2 center)
    {
        //todo implement
        return true;
    }
    public static void CorrectSegmentsToClockwise(this List<LineSegment> segs, Vector2 center)
    {
        if (segs.IsConvexAround(center) == false) throw new Exception();
        for (var i = 0; i < segs.Count; i++)
        {
            if (IsClockwise(segs[i], center) == false) segs[i] = segs[i].GetReverse();
        }
    }
    public static Vector2 GetHullPoint(this List<LineSegment> segs, out int hullPointIndex)
    {
        Vector2 hullPoint = segs[0].From;
        hullPointIndex = 0;
        for (var i = 1; i < segs.Count; i++)
        {
            var p = segs[i].From;
            if (p.x < hullPoint.x)
            {
                hullPoint = p;
                hullPointIndex = i;
            }
            else if (p.x == hullPoint.x && p.y < hullPoint.y)
            {
                hullPoint = p;
                hullPointIndex = i;
            }
        }

        return hullPoint;
    }
    public static bool IsClockwise(this List<LineSegment> segs)
    {
        var hullPoint = GetHullPoint(segs, out var index);
        var prev = segs.Prev(index);
        var seg = segs[index];
        var a = prev.From;
        var c = seg.To;
        return (a - hullPoint).Cross(c - hullPoint) < 0f;
    }

    public static bool IsClockwise(this LineSegment seg, Vector2 center)
    {
        return (center - seg.From).Cross(seg.To - seg.From) > 0f;

    }
    public static List<LineSegment> OrderEndToStart(this List<LineSegment> segs, MapPolygon poly = null)
    {
        var segsSample = segs.ToList();
        var res = new List<LineSegment>{segs[0]};
        segsSample.Remove(segs[0]);
        //scan for next
        var currLast = res.Last();
        var next = segsSample.FirstOrDefault(s => s.From == currLast.To);
        while (next != null && res.Count < segs.Count)
        {
            res.Add(next);
            segsSample.Remove(next);
            currLast = next;
            next = segsSample.FirstOrDefault(s => s.From == currLast.To);
        }
        
        var currFirst = res[0];
        var prevRes = new List<LineSegment>();
        var prev = segsSample.FirstOrDefault(s => s.To == currFirst.From);
        while (prev != null && prevRes.Count + res.Count < segs.Count)
        {
            prevRes.Add(prev);
            segsSample.Remove(prev);
            currFirst = prev;
            prev = segsSample.FirstOrDefault(s => s.To == currFirst.From);
        }

        prevRes.Reverse();
        prevRes.AddRange(res);
        if (prevRes.Count != segs.Count)
        {
            throw new SegmentsNotConnectedException(poly, segs, prevRes, null);
        }
        
        return prevRes;
    }
    public static bool IsContinuous(this List<LineSegment> segs)
    {
        for (var i = 0; i < segs.Count - 1; i++)
        {
            if (segs[i].To != segs[i + 1].From) return false;
        }
        return true;

    }
    public static List<Triangle> Triangulate(this List<LineSegment> boundarySegs,
        HashSet<Vector2> interiorPoints = null)
    {
        if (boundarySegs.IsCircuit() == false) throw new Exception("not circuit");
        if (boundarySegs.IsClockwise() == false) throw new Exception("not clockwise");
        return boundarySegs.Triangulate((v, w, x) => new Triangle(v, w, x), interiorPoints);
    }
    public static List<PolyTri> PolyTriangulate(this List<LineSegment> boundarySegs, Data data, MapPolygon poly,
        HashSet<Vector2> interiorPoints = null)
    {
        return boundarySegs.Triangulate(
            (v, w, x) =>
            {
                var lf = data.Models.Landforms.GetAtPoint(poly, (v + w + x) / 3f, data);
                var vg = data.Models.Vegetation.GetAtPoint(poly, (v + w + x) / 3f, lf, data);
                return new PolyTri(v, w, x, lf, vg);
            }, 
            interiorPoints
        );
    }
    private static List<T> Triangulate<T>(this List<LineSegment> boundarySegs, 
        Func<Vector2, Vector2, Vector2, T> constructor,
        HashSet<Vector2> interiorPoints = null) where T : Triangle
    {
        var points = boundarySegs.GetPoints().GetPoly2TriTriPoints();
        
        if (boundarySegs.IsCircuit() == false)
        {
            GD.Print("not circuit");
            throw new BadTriangulationError(new List<Triangle>(), new List<Color>(), boundarySegs);
        }
        if (boundarySegs.IsClockwise())
        {
            GD.Print("clockwise");
            throw new BadTriangulationError(new List<Triangle>(), new List<Color>(), boundarySegs);
        }
        return Triangulate(points, constructor, interiorPoints);
    }
    private static List<T> Triangulate<T>(this List<TriangulationPoint> points, 
        Func<Vector2, Vector2, Vector2, T> constructor,
        HashSet<Vector2> interiorPoints = null) where T : Triangle
    {
        var boundaryHash = points.ToHashSet();
        var hash = points.Select(p => p.GetV2()).ToHashSet();
        
        if(points.Last().GetV2() == points[0].GetV2()) points.RemoveAt(points.Count - 1);
        
        var constraints = new List<TriangulationConstraint>();
        for (var i = 0; i < points.Count; i++)
        {
            // constraints.Add(new TriangulationConstraint(points[i], points.Next(i)));
            constraints.Add(new TriangulationConstraint(points.Next(i), points[i]));
        } 
        if(interiorPoints != null)
        {
            points.AddRange(interiorPoints.Select(p => p.GetPoly2TriTriPoint()));
            hash.AddRange(interiorPoints);
        }
        points.ForEach(p =>
        {
            if (p == null) throw new Exception();
        });
        var con = new ConstrainedPointSet(points, constraints);
        Poly2Tri.P2T.Triangulate(con);
        

        var tris = new List<T>();
        for (var i = 0; i < con.Triangles.Count; i++)
        {
            var dt = con.Triangles[i];
            var t0 = dt.Points.Item0;
            var t1 = dt.Points.Item1;
            var t2 = dt.Points.Item2;
            var v0 = t0.GetV2();
            var v1 = t1.GetV2();
            var v2 = t2.GetV2();
            
            if (dt.Points.Any(p => hash.Contains(p.GetV2()) == false)) continue;

            if (boundaryHash.Contains(t0)
                && boundaryHash.Contains(t1)
                && boundaryHash.Contains(t2)
                )
            {
                if (TriangleExt.IsClockwise(v0, v1, v2) == false) continue;
                var index = points.IndexOf(t0);    
                var next = points
                    .FindNext(v => v.EqualsV2(v1) || v.EqualsV2(v2), index);
                if (next.EqualsV2(v2) == false) continue;
            }

            var t = constructor(v0, v1, v2);
            tris.Add(t);
        }
        return tris;
    }
    
    
    
    public static IEnumerable<LineSegment> GetLineSegments(this List<Vector2> points, bool close = false)
    {
        
        return Enumerable.Range(0, points.Count() - (close ? 0 : 1))
            .Select(i =>
            {
                return new LineSegment(points[i], points.Modulo(i + 1));
            });
    }

    public static IEnumerable<Vector2> GetPoints(this IEnumerable<LineSegment> pairs)
    {
        var result = Enumerable.Range(0, pairs.Count())
            .Select(i => pairs.ElementAt(i).From)
            .ToList();
        result.Add(pairs.Last().To);
        return result;
    }
    public static Vector2 GetPointAtRatio(this IEnumerable<LineSegment> pairs, float ratio)
    {
        var totalLength = pairs.GetLength();
        var lengthSoFar = 0f;
        var iter = 0;
        var count = pairs.Count();
        while (iter < count)
        {
            var seg = pairs.ElementAt(iter);
            if (lengthSoFar + seg.Length() > totalLength * ratio)
            {
                var portion = totalLength * ratio - lengthSoFar;
                return seg.From.LinearInterpolate(seg.To, portion / seg.Length());
            }
            lengthSoFar += seg.Length();
            iter++;
        }

        throw new Exception();
    }
    
    public static Vector2 GetPointAtLength(this IEnumerable<LineSegment> pairs, float length)
    {
        var totalLength = pairs.GetLength();
        var lengthSoFar = 0f;
        var iter = 0;
        var count = pairs.Count();
        while (iter < count)
        {
            var seg = pairs.ElementAt(iter);
            if (lengthSoFar + seg.Length() > length)
            {
                var portion = totalLength - lengthSoFar;
                return seg.From.LinearInterpolate(seg.To, portion / seg.Length());
            }
            lengthSoFar += seg.Length();
            iter++;
        }

        throw new Exception();
    }

    public static float GetLength(this IEnumerable<LineSegment> pairs)
    {
        return pairs.Select(p => p.From.DistanceTo(p.To)).Sum();
    }
    public static Vector2 GetMiddlePoint(this IEnumerable<LineSegment> pairs)
    {
        var totalLength = pairs.GetLength();
        var lengthSoFar = 0f;
        var iter = 0;
        var count = pairs.Count();
        while (iter < count)
        {
            var seg = pairs.ElementAt(iter);
            if (lengthSoFar + seg.Length() > totalLength / 2f)
            {
                var portion = totalLength / 2f - lengthSoFar;
                return seg.From.LinearInterpolate(seg.To, portion / seg.Length());
            }
            lengthSoFar += seg.Length();
            iter++;
        }

        throw new Exception();
    }

}