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
    
    public static Vector2 FindMiddlePointOfLineSegments(this List<Vector2> points)
    {
        if (points.Count % 2 == 0)
        {
            var left = points[points.Count / 2 - 1];
            var right = points[points.Count / 2];
            return (left + right) / 2f;
        }

        return points[points.Count / 2];
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
    public static Vector2 GetLineIntersection(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        var slopeIntercept1 = GetLineSlopeAndIntercept(p1, p2);
        var slopeIntercept2 = GetLineSlopeAndIntercept(q1, q2);
        var determ = (slopeIntercept1.x * -1f) - (slopeIntercept2.x * -1f);

        if (determ == 0f) throw new Exception("lines are parallel");
        var x = (slopeIntercept1.y - slopeIntercept2.y) / determ;
        var y = (slopeIntercept1.x * -slopeIntercept2.y -  slopeIntercept2.x * -slopeIntercept1.y) / determ;
        return new Vector2(x, y);
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
    public static float GetProjectionLength(Vector2 v, Vector2 onto)
    {
        var angle = v.AngleTo(onto);
        return GetDotProduct(v, onto) / onto.Length();
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

    public static float GetClockwiseAngleTo(this Vector2 v, Vector2 to)
    {
        if (v == to) return 0f;
        var angleTo = v.AngleTo(to);
        return (2f * Mathf.Pi - angleTo) % (2f * Mathf.Pi);
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

    public static IEnumerable<LineSegment> GetLineSegments(this IEnumerable<Vector2> points)
    {
        return Enumerable.Range(0, points.Count() - 1)
            .Select(i =>
            {
                return new LineSegment(points.ElementAt(i), points.ElementAt(i + 1));
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
}
