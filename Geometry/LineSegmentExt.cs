using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Poly2Tri.Triangulation;
using Poly2Tri.Triangulation.Sets;
using Poly2Tri.Utility;

public static class LineSegmentExt
{
    public static Vector2 Average(this IEnumerable<LineSegment> segs)
    {
        var avgX = segs.Average(s => (s.From.x + s.To.x) / 2f);
        var avgY = segs.Average(s => (s.From.y + s.To.y) / 2f);
        var avg = new Vector2(avgX, avgY);
        return avg;
    }
    public static IEnumerable<LineSegment> GetInscribed(this IEnumerable<LineSegment> segs, Vector2 center,
        float insetFactor)
    {
        return segs.Select(s => new LineSegment((s.From - center) * insetFactor, (s.To - center) * insetFactor));
    }
    public static IEnumerable<LineSegment> GetOpposing(this IEnumerable<LineSegment> segs, Vector2 offset)
    {
        return segs.Select(s => s.Translate(-offset).Reverse()).Reverse();
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

    public static void SplitToMinLength(this MapPolygonEdge edge, float minLength, GenWriteKey key)
    {
        var newSegsAbs = new List<LineSegment>();
        var segs = edge.GetSegsAbs();
        var offset = edge.HighId.Entity().GetOffsetTo(edge.LowId.Entity(), key.Data);
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
        edge.ReplacePoints(newSegsAbs, -1, key);
    }
    public static bool IsConvexAround(this List<LineSegment> segs, Vector2 center)
    {
        //todo implement
        return true;
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
    
    
    
    
    public static List<PolyTri> PolyTriangulate(this IReadOnlyList<LineSegment> boundarySegs, GenData data, 
        MapPolygon poly, IdDispenser id,
        HashSet<Vector2> interiorPoints = null)
    {
        return boundarySegs.Triangulate(data, poly,
            (v, w, x) =>
            {
                var lf = data.Models.Landforms.GetAtPoint(poly, (v + w + x) / 3f, data);
                var vg = data.Models.Vegetation.GetAtPoint(poly, (v + w + x) / 3f, lf, data);
                return new PolyTri(v, w, x, lf.MakeRef(), vg.MakeRef());
            }, 
            interiorPoints
        );
    }
    private static List<T> Triangulate<T>(this IReadOnlyList<LineSegment> boundarySegs, GenData data, MapPolygon poly,
        Func<Vector2, Vector2, Vector2, T> constructor, 
        HashSet<Vector2> interiorPoints = null) where T : Triangle
    {
        if (boundarySegs.IsCircuit() == false)
        {
            GD.Print("not circuit");
            throw new BadTriangulationException(poly, new List<Triangle>(), new List<Color>(), data, boundarySegs.ToList());
        }
        
        var points = boundarySegs.GetPoints().GetPoly2TriTriPoints();
        var boundaryHash = points.Select(p => p.GetV2()).ToHashSet();
        var hash = points.Select(p => p.GetV2()).ToHashSet();
        
        if(points.Last().GetV2() == points[0].GetV2()) points.RemoveAt(points.Count - 1);
        
        var constraints = new List<TriangulationConstraint>();
        for (var i = 0; i < points.Count; i++)
        {
            constraints.Add(new TriangulationConstraint(points.Next(i), points[i]));
        } 
        if(interiorPoints != null)
        {
            points.AddRange(interiorPoints.Select(p => p.GetPoly2TriTriPoint()));
            hash.AddRange(interiorPoints);
        }
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

            if (boundaryHash.Contains(v0)
                && boundaryHash.Contains(v1)
                && boundaryHash.Contains(v2)
                )
            {
                var index = points.IndexOf(t0);    
                var next = points
                    .FindNext(v => v.EqualsV2(v1) || v.EqualsV2(v2), index);
                if (next.EqualsV2(v1) == false) continue;
            }

            var t = constructor(v0, v1, v2);
            tris.Add(t);
        }
        //todo check for missing
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

    public static Vector2 GetPointAtLength(this LineSegment ls, float length)
    {
        if (length > ls.Length()) return ls.To;
        return ls.From + (ls.To - ls.From).Normalized() * length;
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

    public static Vector2 GetCornerPoint(this LineSegment l1, LineSegment l2, float thickness)
    {
        var angle = l1.AngleBetween(l2);
        var axis = l1.GetNormalizedAxis();
        var perp = -axis.Perpendicular() * thickness;
        if (angle > Mathf.Pi) angle = -angle;
        var d = thickness / Mathf.Tan(angle / 2f);
        var p = l1.To - axis * d + perp; 

        return p;
    }
    public static float AngleBetween(this LineSegment l1, LineSegment l2)
    {
        if (l1.To != l2.From) throw new Exception();
        return (l1.From - l1.To).GetClockwiseAngleTo(l2.To - l2.From);
    }
}