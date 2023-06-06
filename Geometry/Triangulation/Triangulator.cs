using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DelaunatorSharp;
using MIConvexHull;
using Poly2Tri;


public static class Triangulator
{
    public static ConcurrentBag<int> InteriorPointGenTimes = new ConcurrentBag<int>();
    public static ConcurrentBag<int> P2TTriangulateTimes = new ConcurrentBag<int>();
    public static ConcurrentBag<int> TotalPolyTriangulateTimes = new ConcurrentBag<int>();
    public static ConcurrentBag<int> ConstructPolyTriTimes = new ConcurrentBag<int>();
    public static ConcurrentBag<int> FindLfAndVTimes = new ConcurrentBag<int>();
    
    public static List<PolyTri> PolyTriangulate(this Vector2[] boundaryPoints, Data data, MapPolygon poly)
    {
        var sw1 = new Stopwatch();
        sw1.Start();
        var swLfV = new Stopwatch();
        Func<Vector2, Vector2, Vector2, PolyTri> constructor = (v, w, x) =>
        {
            swLfV.Start();
            var lf = data.Models.Landforms.GetAtPoint(poly, (v + w + x) / 3f, data);
            var vg = data.Models.Vegetation.GetAtPoint(poly, (v + w + x) / 3f, lf, data);
            swLfV.Stop();
            FindLfAndVTimes.Add((int)swLfV.Elapsed.TotalMilliseconds);
            swLfV.Reset();
            return PolyTri.Construct(v, w, x, lf.MakeRef(), vg.MakeRef());
        };
        var polygon = new Poly2Tri.Polygon(boundaryPoints.Select(p => new PolygonPoint(p.x, p.y)));

        var sw = new Stopwatch();
        
        sw.Start();
        boundaryPoints.GenerateInteriorPoints(30f, 10f, 
            v => polygon.AddSteinerPoint(new TriangulationPoint(v.x, v.y)));
        sw.Stop();
        InteriorPointGenTimes.Add((int)sw.Elapsed.TotalMilliseconds);
        sw.Reset();
        
        var sweep = new Poly2Tri.DTSweepContext();
        for (var i = 0; i < boundaryPoints.Length; i++)
        {
            var seg = boundaryPoints[i];
            var next = boundaryPoints.Modulo(i + 1);
            sweep.NewConstraint(new TriangulationPoint(seg.x, seg.y),
                new TriangulationPoint(next.x, next.y));
        }
        polygon.Prepare(sweep);
        
        sw.Start();
        P2T.Triangulate(polygon);
        sw.Stop();
        P2TTriangulateTimes.Add((int)sw.Elapsed.TotalMilliseconds);
        sw.Reset();
        
        sw.Start();
        var tris = new List<PolyTri>{};
        foreach (var t in polygon.Triangles)
        {
            var a = new Vector2(t.Points[0].Xf, t.Points[0].Yf);
            var b = new Vector2(t.Points[1].Xf, t.Points[1].Yf);
            var c = new Vector2(t.Points[2].Xf, t.Points[2].Yf);
            var center = (a + b + c) / 3f;

            //cache poly border tris and check against them?
            if (Geometry.IsPointInPolygon(center, boundaryPoints))
            {
                tris.Add(constructor(a, b, c));
            }
        }
        sw.Stop();
        ConstructPolyTriTimes.Add((int)sw.Elapsed.TotalMilliseconds);
        sw.Reset();
        
        
        sw1.Stop();
        TotalPolyTriangulateTimes.Add((int)sw1.Elapsed.TotalMilliseconds);
        return tris;
    }
    
    private static float GetAreaFromBoundary(Vector2[] boundary)
    {
        var triIndices = Geometry.TriangulatePolygon(boundary);
        if (triIndices.Length == 0) return Mathf.Inf;
        var res = 0f;
        for (var i = 0; i < triIndices.Length; i += 3)
        {
            res += TriangleExt.GetTriangleArea(boundary[triIndices[i]],
                boundary[triIndices[i + 1]],
                boundary[triIndices[i + 2]]);
        }

        return res;
    }
    private static List<ColorTri> GetTrisFromBoundary(Vector2[] boundary, Color col)
    {
        var triIndices = Geometry.TriangulatePolygon(boundary);
        if (triIndices.Length == 0) return new List<ColorTri>();
        var tris = new List<ColorTri>();
        for (var i = 0; i < triIndices.Length; i += 3)
        {
            tris.Add(new ColorTri(col.GetPeriodicShade(i),
                boundary[triIndices[i]], 
                boundary[triIndices[i + 1]], 
                boundary[triIndices[i + 2]]));
        }

        return tris;
    }
    
    public static List<Triangle> TriangulatePoints(List<Vector2> points)
    {
        var d = new Delaunator(points.Select(p => new DelaunatorPoint(p)).ToArray());
        var tris = new List<Triangle>();
        for (int i = 0; i < d.Triangles.Length; i+=3)
        {
            var triIndex = i / 3;
            var pointId1 = d.Triangles[i];
            var dPoint1 = d.Points[pointId1];
            
            var pointId2 = d.Triangles[i + 1];
            var dPoint2 = d.Points[pointId2];
            
            var pointId3 = d.Triangles[i + 2];
            var dPoint3 = d.Points[pointId3];
            var adj = d.TrianglesAdjacentToTriangle(triIndex);
            
            tris.Add(new Triangle(dPoint1.GetV2(), dPoint2.GetV2(), dPoint3.GetV2()));
        }
        return tris;
    }
    
    
    public class DelaunatorPoint : IPoint
    {
        public double X {get; set;}
        public double Y {get; set;}

        public DelaunatorPoint(Vector2 v)
        {
            X = (int)v.x;
            Y = (int)v.y;
        }
    }
}
public static class IPointExt
{
    public static IPoint GetIPoint(this Vector2 v)
    {
        return new Triangulator.DelaunatorPoint(v);
    }
    public static Vector2 GetV2(this IPoint p)
    {
        return new Vector2((float) p.X, (float) p.Y);
    }
    public static Vector2 GetIntV2(this IPoint p)
    {
        return new Vector2(Mathf.FloorToInt((float)p.X), Mathf.FloorToInt((float)p.Y));
    }
}