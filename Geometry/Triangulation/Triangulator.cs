using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using MIConvexHull;
using Poly2Tri;


public static class Triangulator
{
    public static List<PolyTri> PolyTriangulate(this Vector2[] boundaryPoints, Data data, MapPolygon poly)
    {
        Func<Vector2, Vector2, Vector2, PolyTri> constructor = (v, w, x) =>
        {
            var lf = data.Models.Landforms.GetAtPoint(poly, (v + w + x) / 3f, data);
            var vg = data.Models.Vegetation.GetAtPoint(poly, (v + w + x) / 3f, lf, data);
            return PolyTri.Construct(v, w, x, lf.MakeRef(), vg.MakeRef());
        };
        var polygon = new Poly2Tri.Polygon(boundaryPoints.Select(p => new PolygonPoint(p.x, p.y)));

        boundaryPoints.GenerateInteriorPoints(30f, 10f, 
            v => polygon.AddSteinerPoint(new TriangulationPoint(v.x, v.y)));
        
        var sweep = new Poly2Tri.DTSweepContext();
        for (var i = 0; i < boundaryPoints.Length; i++)
        {
            var seg = boundaryPoints[i];
            var next = boundaryPoints.Modulo(i + 1);
            sweep.NewConstraint(new TriangulationPoint(seg.x, seg.y),
                new TriangulationPoint(next.x, next.y));
        }
        polygon.Prepare(sweep);
        P2T.Triangulate(polygon);
        try
        {
            
        }
        catch
        {
            // var e = new GeometryException("p2t failed");
            // e.AddSegLayer(boundarySegs, "boundary");
            // throw e;
            return new List<PolyTri>{constructor(Vector2.One, Vector2.Left, Vector2.Right)};
        }
        
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