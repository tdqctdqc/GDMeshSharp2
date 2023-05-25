using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GeometRi;

public static class Vector2Ext
{
    public static List<Vector2> GeneratePointsAlong(this LineSegment seg, float spacing, 
        float variation, List<Vector2> addTo = null)
    {
        if (addTo == null) addTo = new List<Vector2>();
        var to = seg.To;
        var from = seg.From;
        if (variation >= spacing / 2f) throw new Exception();
        var numPoints = Mathf.FloorToInt((to - from).Length() / spacing );
        var axis = (to - from).Normalized();
        for (int i = 1; i <= numPoints; i++)
        {
            var rand = Game.I.Random.RandfRange(-1f, 1f);
            var p = from + axis * (spacing * i);
            addTo.Add(p);
        }

        return addTo;
    }
    
    public static List<Vector2> GeneratePointsAlong(this Vector2 to, float spacing, float variation, bool includeEndPoints,
            List<Vector2> addTo = null, Vector2 from = default)
    {
        if (variation >= spacing / 2f) throw new Exception();
        if (addTo == null) addTo = new List<Vector2>();

        
        if(includeEndPoints) addTo.Add(from);
        var numPoints = Mathf.FloorToInt((to - from).Length() / spacing ) - 1;
        var axis = (to - from).Normalized();
        for (int i = 1; i <= numPoints; i++)
        {
            var rand = Game.I.Random.RandfRange(-1f, 1f);
            var p = from + axis * (spacing * i);
            addTo.Add(p);
        }
        if(includeEndPoints) addTo.Add(to);

        return addTo;
    }
    public static Vector2 Shorten(this Vector2 v, float shortenBy)
    {
        if (v.Length() <= shortenBy) throw new Exception();
        return v.Normalized() * (v.Length() - shortenBy);
    }
    public static Vector2 Intify(this Vector2 v)
    {
        return new Vector2(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y));
    }
    public static Vector2 Avg(this List<Vector2> points)
    {
        var res = Vector2.Zero;
        points.ForEach(p => res += p);
        return res / points.Count;
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

    public static bool PointIsOnLine(this Vector2 point, Vector2 from, Vector2 to)
    {
        return from.Cross(to) == 0;
    }
    public static bool PointIsInLineSegment(this Vector2 point, Vector2 from, Vector2 to)
    {
        if (point.PointIsOnLine(from, to) == false) return false;
        return point.x >= Mathf.Min(from.x, to.x)
                && point.x <= Mathf.Max(from.x, to.x)
                && point.y >= Mathf.Min(from.y, to.y)
                && point.y <= Mathf.Max(from.y, to.y);
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
            intersect = new Vector2(Single.NaN, Single.NaN);
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
        if (PointIsInLineSegment(point, p1, p2)
            && PointIsInLineSegment(point, q1, q2))
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

    public static Vector2 ClampToBox(this Vector2 p, Vector2 bound1, Vector2 bound2)
    {
        var minXBound = Mathf.Min(bound1.x, bound2.x);
        var minYBound = Mathf.Min(bound1.y, bound2.y);
        var maxXBound = Mathf.Max(bound1.x, bound2.x);
        var maxYBound = Mathf.Max(bound1.y, bound2.y);

        return new Vector2(Mathf.Clamp(p.x, minXBound, maxXBound),
            Mathf.Clamp(p.y, minYBound, maxYBound));
    }
    public static float DistToLine(this Vector2 point, Vector2 start, Vector2 end)
    {
        var theta = Mathf.Abs((point - start).AngleTo(end - start));
        return Mathf.Sin(theta) * point.DistanceTo(start);
        
        
        
        
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
    
}
