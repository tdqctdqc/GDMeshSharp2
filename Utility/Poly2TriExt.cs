using System.Collections.Generic;
using System.Linq;
using Poly2Tri;
using Godot;
using Poly2Tri.Triangulation;
using Poly2Tri.Triangulation.Delaunay;
using Poly2Tri.Triangulation.Polygon;
using Poly2Tri.Utility;

// using Poly2Tri.Triangulation;
// using Poly2Tri.Triangulation.Delaunay;
// using Poly2Tri.Triangulation.Polygon;
// using Poly2Tri.Utility;

public static class Poly2TriExt
{
    public static bool EqualsV2(this Point2D p, Vector2 v)
    {
        return p.Xf == v.x && p.Yf == v.y;
    }
    public static Vector2 GetV2(this Point2D p)
    {
        return new Vector2(p.Xf, p.Yf);
    }
    
    public static TriangulationPoint GetPoly2TriTriPoint(this Vector2 p)
    {
        return new TriangulationPoint(p.x, p.y);
    }
    //
    public static PolygonPoint GetPoly2TriPolyPoint(this Vector2 p)
    {
        return new PolygonPoint(p.x, p.y);
    }
    public static List<PolygonPoint> GetPoly2TriPolyPoints(this List<Vector2> p)
    {
        return p.Select( v => new PolygonPoint(v.x, v.y)).ToList();
    }
    
    public static List<TriangulationPoint> GetPoly2TriTriPoints(this IEnumerable<Vector2> p)
    {
        return p.Select( v => new TriangulationPoint(v.x, v.y)).ToList();
    }
    public static Triangle GetTri(this DelaunayTriangle dt)
    {
        return new Triangle(
            new Vector2(dt.Points.Item0.Xf, dt.Points.Item0.Yf),
            new Vector2(dt.Points.Item1.Xf, dt.Points.Item1.Yf),
            new Vector2(dt.Points.Item2.Xf, dt.Points.Item2.Yf)
        );
    }
}
