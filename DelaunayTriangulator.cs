using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorNetStd;

public class DelaunayTriangulator
{
    public static List<Vector2> TriangulatePoints(List<Vector2> points)
    {
        var delaunayPoints = new List<IPoint>();
        foreach (var p in points)
        {
            var delaunayPoint = new DelaunatorPoint(p);
            delaunayPoints.Add(delaunayPoint);
        }
        var d = new Delaunator(delaunayPoints.ToArray());
        var tris = new List<Vector2>();
        for (int i = 0; i < d.Triangles.Length; i++)
        {
            var pointId = d.Triangles[i];
            var dPoint = d.Points[pointId];
            tris.Add(new Vector2((float)dPoint.X, (float)dPoint.Y));
        }
        return tris;
    }

    
    public class DelaunatorPoint : IPoint
    {
        public double X {get; set;}
        public double Y {get; set;}

        public DelaunatorPoint(Vector2 v)
        {
            X = v.x;
            Y = v.y;
        }
    }
}
public static class IPointExt
{
    public static Vector2 GetV2(this IPoint p)
    {
        return new Vector2((float) p.X, (float) p.Y);
    }
    public static Vector2 GetIntV2(this IPoint p)
    {
        return new Vector2(Mathf.FloorToInt((float)p.X), Mathf.FloorToInt((float)p.Y));
    }
}