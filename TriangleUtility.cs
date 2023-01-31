using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class TriangleUtility 
{
    public static List<Vector2> GetTriPoints(this List<Triangle> tris)
    {
        var res = new List<Vector2>();
        for (var i = 0; i < tris.Count; i++)
        {
            res.Add(tris[i].A);
            res.Add(tris[i].B);
            res.Add(tris[i].C);
        }
        return res;
    }
    public static bool BadTri(this Triangle tri, float minLength)
    {
        return BadTri(minLength, tri.A, tri.B, tri.C);
    }
    public static bool BadTri(float minLength, Vector2 a, Vector2 b, Vector2 c)
    {
        if (GetMinAltitude(a,b,c) < minLength
            || GetMinEdgeLength(a,b,c) < minLength)
        {
            return true;
        }
        return false;
    }
    public static float GetTriangleArea(Vector2 a, Vector2 b, Vector2 c)
    {
        return .5f * Mathf.Abs(a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y));
    }

    public static void AddTriPointsToCollection(this Triangle tri, ICollection<Vector2> col)
    {
        col.Add(tri.A);
        col.Add(tri.B);
        col.Add(tri.C);
    }
    
    public static float GetMinEdgeLength(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        var dist1 = p0.DistanceTo(p1);
        var dist2 = p0.DistanceTo(p2);
        var dist3 = p1.DistanceTo(p2);
        float min = Mathf.Min(dist1, dist2);
        return Mathf.Min(min, dist3);
    }

    public static float GetMinAltitude(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        var area = GetArea(p0, p1, p2);
        var minAlt = Mathf.Inf;
        float altitude(Vector2 po0, Vector2 po1)
        {
            var baseLength = po1.DistanceTo(po0);
            return area * 2f / baseLength;
        }

        minAlt = Mathf.Min(minAlt, altitude(p0, p1));
        minAlt = Mathf.Min(minAlt, altitude(p1, p2));
        minAlt = Mathf.Min(minAlt, altitude(p2, p0));
        return minAlt;
    }
    public static float GetMinAltitude(List<Vector2> points)
    {
        return GetMinAltitude(points[0], points[1], points[2]);
    }

    public static float GetArea(this Triangle t)
    {
        return GetArea(t.A, t.B, t.C);
    }

    public static Vector2 GetRandomPointInside(this Triangle t, float minArcRatio, float maxArcRatio)
    {
        var arc1 = t.B - t.A;
        var arc2 = t.C - t.A;
        var totalArcRatio = Game.I.Random.RandfRange(minArcRatio, maxArcRatio);
        var arc1Ratio = Game.I.Random.RandfRange(0f, totalArcRatio);
        var arc2Ratio = totalArcRatio - arc1Ratio;
        return t.A + arc1 * arc1Ratio + arc2 * arc2Ratio;
    }
    public static float GetArea(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        var l0 = p0.DistanceTo(p1);
        var l1 = p1.DistanceTo(p2);
        var l2 = p2.DistanceTo(p0);
        var semiPerim = (l0 + l1 + l2) / 2f;
        return Mathf.Sqrt( semiPerim * (semiPerim - l0) * (semiPerim - l1) * (semiPerim - l2) );
    }

    public static Vector2 GetCircumcenter(Vector2 a, Vector2 b, Vector2 c)
    {
        var ad = a[0] * a[0] + a[1] * a[1];
        var bd = b[0] * b[0] + b[1] * b[1];
        var cd = c[0] * c[0] + c[1] * c[1];
        var D = 2 * (a[0] * (b[1] - c[1]) + b[0] * (c[1] - a[1]) + c[0] * (a[1] - b[1]));
        return new Vector2
        (
            1 / D * (ad * (b[1] - c[1]) + bd * (c[1] - a[1]) + cd * (a[1] - b[1])),
            1 / D * (ad * (c[0] - b[0]) + bd * (a[0] - c[0]) + cd * (b[0] - a[0]))
        );
    }

    public static bool IsDegenerate(this Triangle tri)
    {
        if ((tri.B - tri.A).Normalized() == (tri.C - tri.A).Normalized()) return true;
        return false;
    }
    public static bool PointInsideTriangle(this Triangle tri, Vector2 p)
    {
        bool hasNeg, hasPos;
        var v1 = tri.A;
        var v2 = tri.B;
        var v3 = tri.C;
        float sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }
        var d1 = sign(p, v1, v2);
        var d2 = sign(p, v2, v3);
        var d3 = sign(p, v3, v1);

        hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }
}