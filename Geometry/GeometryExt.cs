using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class GeometryExt
{
    public static Vector2 Intify(this Vector2 v)
    {
        return new Vector2((int) v.x, (int) v.y);
    }

    public static Vector2 Avg(this IEnumerable<Vector2> v)
    {
        return Sum(v) / v.Count();
    }
    public static Vector2 Sum(this IEnumerable<Vector2> v)
    {
        var r = Vector2.Zero;
        foreach (var vector2 in v)
        {
            r += vector2;
        }

        return r;
    }
    public static (Vector2 p1, Vector2 p2, float dist) FindClosestPointPair(this List<Vector2> p1s, List<Vector2> p2s)
    {
        float minDist = Mathf.Inf;
        var closeP1 = Vector2.Zero;
        var closeP2 = Vector2.Zero;
        foreach (var p1 in p1s)
        {
            foreach (var p2 in p2s)
            {
                var dist = p1.DistanceTo(p2);
                if (dist < minDist)
                {
                    minDist = Mathf.Min(dist, minDist);
                    closeP1 = p1;
                    closeP2 = p2;
                }
            }
        }
        return (closeP1, closeP2, minDist);
    }
    
    public static float GridDistanceTo(this Vector2 point0, Vector2 point1)
    {
        return Mathf.Abs(point0.x - point1.x) + Mathf.Abs(point0.y - point1.y);
    }
    public static bool PointIsLeftOfLine(this Vector2 point, Vector2 line1, Vector2 line2){
        return ((line2.x - line1.x)*(point.y - line1.y) - (line2.y - line1.y)*(point.x - line1.x)) > 0;
    }
    public static float GetDotProduct(Vector2 a, Vector2 b)
    {
        return a.x * b.x + a.y + b.y;
    }

    public static bool PointOnLineIsInLineSegment(this Vector2 point, Vector2 from, Vector2 to)
    {
        
        if (point.x > Mathf.Max(from.x, to.x)
            || point.x < Mathf.Min(from.x, to.x)
            || point.y > Mathf.Max(from.y, to.y)
            || point.y < Mathf.Min(from.y, to.y))
        {
            return false;
        }

        return true;
    }

    public static bool LineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        if (GetLineIntersection(p1, p2, q1, q2, out var intersect))
        {
            var maxPX = Mathf.Max(p1.x, p2.x);
            var minPX = Mathf.Min(p1.x, p2.x);
            var maxQX = Mathf.Max(q1.x, q2.x);
            var minQX = Mathf.Min(q1.x, q2.x);
            return intersect.x <= maxPX && intersect.x >= minPX
                                        && intersect.x <= maxQX && intersect.x >= minQX;
        }

        return false;
    }
    public static bool GetLineIntersection(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2, out Vector2 intersect)
    {
        if (p1.x == p2.x)
        {
            if (q1.x == q2.x)
            {
                //either same line or no intersection
                intersect = Vector2.Inf;
                return false;
            }

            return GetIntersectionForVertical(p1.x, q1, q2, out intersect);
        }
        if (q1.x == q2.x)
        {
            return GetIntersectionForVertical(q1.x, p1, p2, out intersect);
        }
    
        var slopeIntercept1 = GetLineSlopeAndIntercept(p1, p2);
        var slopeIntercept2 = GetLineSlopeAndIntercept(q1, q2);
        var determ = (slopeIntercept1.x * -1f) - (slopeIntercept2.x * -1f);

        if (determ == 0f)
        {
            intersect = Vector2.Inf;
            return false;
        }
        var x = (slopeIntercept1.y - slopeIntercept2.y) / determ;
        var y = (slopeIntercept1.x * -slopeIntercept2.y -  slopeIntercept2.x * -slopeIntercept1.y) / determ;
        intersect = new Vector2(x, y);

        return true;
    }

    private static bool GetIntersectionForVertical(float x, Vector2 p1, Vector2 p2, out Vector2 intersect)
    {
        var left = Mathf.Min(p1.x, p2.x);
        var right = Mathf.Max(p1.x, p2.x);
        var bottom = Mathf.Min(p1.y, p2.y);
        var top = Mathf.Max(p1.y, p2.y);

        var dist = Mathf.Abs(right - left);
        var ratio = (x - left) / dist;
        intersect = new Vector2(x, bottom + Mathf.Abs(top - bottom) * ratio);
        return true;
    }
    public static Vector2? GetLineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        var slopeIntercept1 = GetLineSlopeAndIntercept(p1, p2);
        var slopeIntercept2 = GetLineSlopeAndIntercept(q1, q2);
        var determ = (slopeIntercept1.x * -1f) - (slopeIntercept2.x * -1f);

        if (determ == 0f) return null;
        var x = (slopeIntercept1.y - slopeIntercept2.y) / determ;
        var y = (slopeIntercept1.x * -slopeIntercept2.y -  slopeIntercept2.x * -slopeIntercept1.y) / determ;
        var point = new Vector2(x, y);
        if (PointOnLineIsInLineSegment(point, p1, p2)
            && PointOnLineIsInLineSegment(point, q1, q2))
        {
            return point;
        }
        
        return null;
    }
    
    public static Vector2 GetLineSlopeAndIntercept(Vector2 p1, Vector2 p2)
    {
        var left = p1.x < p2.x
            ? p1
            : p2;
        var right = p1.x < p2.x
            ? p2
            : p1;
        var slope = (right.y - left.y) / (right.x - left.x);

        var intercept = p1.y - slope * p1.x;
        return new Vector2(slope, intercept);
    }
    public static float GetProjectionLength(this Vector2 v, Vector2 onto)
    {
        var angle = v.AngleTo(onto);
        return v.Dot(onto) / onto.Length();
    }
    public static Vector2 GetReflectionOfPointAcrossLine(this Vector2 point, Vector2 from, Vector2 to)
    {
        var fp = point - from;
        return to - fp;
    }
    public static float DistToLine(this Vector2 point, Vector2 start, Vector2 end)
    {
        // vector AB
        var AB = new Vector2();
        AB.x = end.x - start.x;
        AB.y = end.y - start.y;
 
        // vector BP
        var BE = new Vector2();
        BE.x = point.x - end.x;
        BE.y = point.y - end.y;
 
        // vector AP
        var AE = new Vector2();
        AE.x = point.x - start.x;
        AE.y = point.y - start.y;
 
        // Variables to store dot product
        float AB_BE, AB_AE;
 
        // Calculating the dot product
        AB_BE = (AB.x * BE.x + AB.y * BE.y);
        AB_AE = (AB.x * AE.x + AB.y * AE.y);
 
        // Minimum distance from
        // point E to the line segment
        float reqAns = 0;
 
        // Case 1
        if (AB_BE > 0)
        {
 
            // Finding the magnitude
            var y = point.y - end.y;
            var x = point.x - end.x;
            reqAns = Mathf.Sqrt(x * x + y * y);
        }
 
        // Case 2
        else if (AB_AE < 0)
        {
            var y = point.y - start.y;
            var x = point.x - start.x;
            reqAns = Mathf.Sqrt(x * x + y * y);
        }
 
        // Case 3
        else
        {
 
            // Finding the perpendicular distance
            var x1 = AB.x;
            var y1 = AB.y;
            var x2 = AE.x;
            var y2 = AE.y;
            var mod = Mathf.Sqrt(x1 * x1 + y1 * y1);
            reqAns = Mathf.Abs(x1 * y2 - y1 * x2) / mod;
        }
        return reqAns;
    }
    public static float GetClockwiseAngle(this Vector2 v)
    {
        if (v == Vector2.Zero) return 0f;
        return (2f * Mathf.Pi + v.Angle()) % (2f * Mathf.Pi);
    }
    public static float GetClockwiseAngleTo(this Vector2 v, Vector2 to)
    {
        if (v == to) return 0f;
        return (2f * Mathf.Pi + v.AngleTo(to)) % (2f * Mathf.Pi);
    }

    public static float RadToDegrees(this float rad)
    {
        return (360f * rad / (Mathf.Pi * 2f));
    }

    public static List<Vector2> StitchTogether(this List<List<Vector2>> segments)
    {
        var res = new List<Vector2>(segments[0]);
        for (int i = 1; i < segments.Count; i++)
        {
            var seg = segments[i];
            if (seg[0] == res[res.Count - 1])
            {
                for (var j = 1; j < seg.Count; j++)
                {
                    res.Add(seg[j]);
                }
            }
            else if (seg[seg.Count - 1] == res[res.Count - 1])
            {
                for (var j = seg.Count - 2; j >= 0; j--)
                {
                    res.Add(seg[j]);
                }
            }
            // else throw new Exception();
        }

        return res;
    }

    public static Vector2 Avg(this List<Vector2> points)
    {
        var res = Vector2.Zero;
        points.ForEach(p => res += p);
        return res / points.Count;
    }

    public static IEnumerable<LineSegment> GetLineSegments(this IEnumerable<Vector2> points, bool close = false)
    {
        return Enumerable.Range(0, points.Count() - (close ? 0 : 1))
            .Select(i =>
            {
                return new LineSegment(points.ElementAt(i), points.ElementAt((i + 1) % points.Count()));
            });
    }

    public static LineSegment GetFirst(this IEnumerable<LineSegment> segments)
    {
        var noTo = segments.Where(s => segments.Any(n => n.PointsTo(s) == false));
        if (noTo.Count() != 1) throw new Exception();
        return segments.First(s => segments.Any(n => n.PointsTo(s) == false));
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

    public static Vector2 Shorten(this Vector2 v, float shortenBy)
    {
        if (v.Length() <= shortenBy) throw new Exception();
        return v.Normalized() * (v.Length() - shortenBy);
    }

    public static bool IsOnLineSegment(this Vector2 p, Vector2 to, Vector2 from = default)
    {
        return (p - from).Normalized() == (to - from).Normalized();
    }
    public static List<Triangle> ToTriangles(this List<Vector2> triPoints)
    {
        var result = new List<Triangle>();
        for (var i = 0; i < triPoints.Count; i+=3)
        {
            result.Add(new Triangle(triPoints[i], triPoints[i + 1], triPoints[i + 2]));
        }
        return result;
    }

    public static List<Vector2> GeneratePointsAlong(this Vector2 to, float spacing, float variation, Vector2 from = default)
    {
        if (variation >= spacing / 2f) throw new Exception();
        var res = new List<Vector2>();
        var numPoints = Mathf.FloorToInt((to - from).Length() / spacing );
        var axis = (to - from).Normalized();
        for (int i = 1; i <= numPoints; i++)
        {
            var rand = Game.I.Random.RandfRange(-1f, 1f);
            var p = from + axis * (spacing * i);
            res.Add(p);
        }

        return res;
    }

    public static List<Triangle> TriangulateSegment(this List<LineSegment> segs, 
        LineSegment startLeg, LineSegment endLeg)
    {
        if (startLeg.From != endLeg.To) throw new Exception();
        var anchor = startLeg.From;
        var tris = new List<Triangle>();

        var partitionIndices = new List<int>{};
        var partitionNewPoints = new List<List<Vector2>>();
        var startRay = startLeg.To - anchor;
        var startNewPoints = startLeg.To.GeneratePointsAlong(50f, 5f, anchor);
        var endNewPoints = endLeg.From.GeneratePointsAlong(50f, 5f, anchor);
        var partitionNewPointsAll = new List<Vector2>();
        for (var i = 0; i < segs.Count; i++)
        {
            var seg = segs[i];

            var ray = seg.To - anchor;

            if (startRay.GetClockwiseAngleTo(ray) > Mathf.Pi)
            {
                partitionIndices.Add(i);
                var newPs = seg.To.GeneratePointsAlong(50f, 5f, anchor);
                partitionNewPoints.Add(newPs);
                partitionNewPointsAll.AddRange(newPs);
                startRay = ray;
            }
        }

        if (partitionIndices.Count > 0)
        {
            var prev = 0;
            var prevPoints = startNewPoints;
            var start = startLeg.To;
            for (var i = 0; i < partitionIndices.Count; i++)
            {
                var partIndex = partitionIndices[i];
                var sliceSegs = segs.GetRange(prev, partIndex - prev);
                var end = segs[partIndex].From;
                var startSeg = new LineSegment(anchor, start);
                var endSeg = new LineSegment(end, anchor);
                
                sliceSegs.TriangulateConcaveSegment(startSeg, endSeg, tris, prevPoints, new List<Vector2>());
                prevPoints = partitionNewPoints[i];
                start = end;
                prev = partIndex;
            }

            var lastSeg = segs[partitionIndices.Last()];
            var lastStartSeg = new LineSegment(anchor, lastSeg.From);
            var lastSliceSegs = segs.GetRange(partitionIndices.Last(), segs.Count - partitionIndices.Last());
            var lastEndSeg = new LineSegment(lastSeg.To, anchor);
            lastSliceSegs.TriangulateConcaveSegment(lastStartSeg, lastEndSeg, tris, prevPoints, endNewPoints);
        }
        else
        {
            TriangulateConcaveSegment(segs, startLeg, endLeg, tris, startNewPoints, endNewPoints);
        }
        
        partitionNewPointsAll.ForEach(p =>
        {
            var pTris = tris.Where(t => t.HasPoint(p));
            var avg = pTris.Select(t => (t.A + t.B + t.C) / 3).Avg();
            foreach (var pTri in pTris)
            {
                pTri.ReplacePoint(p, avg);
            }
        });
        
        
        
        return tris;
    }

    public static void TriangulateConcaveSegment(this List<LineSegment> segs,
        LineSegment startLeg, LineSegment endLeg, List<Triangle> tris, List<Vector2> newStartPoints, 
        List<Vector2> newEndPoints)
    {
        var segs2 = segs.Union(new List<LineSegment>(){
            startLeg
        }).Union(new List<LineSegment>{
            endLeg
        }).Distinct().ToList();
        var points = segs.GetPoints().ToList();
        
        points.AddRange(segs2.GenerateInteriorPoints(50f).Where(p => endLeg.DistanceTo(p) > 10f
                                                                    && startLeg.DistanceTo(p) > 10f
                                                                    && segs.Min(s => s.DistanceTo(p) > 10f)
        ));
        points.AddRange(newStartPoints);
        points.AddRange(newEndPoints);
        points.Add(startLeg.To);
        points.Add(startLeg.From);
        points.Add(endLeg.To);
        points.Add(endLeg.From);
        tris.AddRange(
            DelaunayTriangulator.TriangulatePoints(points.Distinct().ToList())
                .ToTriangles()
                .Where(t => t.IsDegenerate() == false)
            );
        
    }
}
