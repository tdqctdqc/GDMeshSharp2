
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Poly2Tri;
using Poly2Tri.Triangulation;
using Poly2Tri.Triangulation.Polygon;
using Poly2Tri.Triangulation.Sets;

public class TriangulationTesting
{
    private SandboxClient _client;

    public TriangulationTesting(SandboxClient client)
    {
        _client = client;
    }

    public void Run()
    {
        var mult = 100f;
        var p0_0 = new Vector2(0f * mult, 0f * mult);
        var p1_0 = new Vector2(1f * mult, 0f * mult);
        var p1_1 = new Vector2(1f * mult, 1f * mult);
        var p0_1 = new Vector2(0f * mult, 1f * mult);
        var p2_0 = new Vector2(2f * mult, 0f * mult);
        var p2_1= new Vector2(2f * mult, 1f * mult);
        var p2_2= new Vector2(2f * mult, 2f * mult);
        var p1_2= new Vector2(1f * mult, 2f * mult);
        var p3_1= new Vector2(3f * mult, 1f * mult);
        var p3_0= new Vector2(3f * mult, 0f * mult);
        
        var segs = new List<LineSegment>
        {
            new LineSegment(p0_0, p1_0),
            new LineSegment(p1_0, p2_0),
            new LineSegment(p2_0, p3_0),
            new LineSegment(p3_0, p3_1),
            new LineSegment(p3_1, p2_1),
            new LineSegment(p2_1, p2_2),
            new LineSegment(p2_2, p1_2),
            new LineSegment(p1_2, p1_1),
            new LineSegment(p1_1, p0_1),
            new LineSegment(p0_1, p0_0),
        };
        
        var squareSegs = new List<LineSegment>
        {
            new LineSegment(p0_0, p1_0),
            new LineSegment(p1_0, p1_1),
            new LineSegment(p1_1, p0_1),
            new LineSegment(p0_1, p0_0),
        };

        var interiorPs = new HashSet<Vector2>
        {
            new Vector2(.5f * mult, .5f * mult),
            new Vector2(1.5f * mult, .5f * mult),
            new Vector2(2.5f * mult, .5f * mult),
            new Vector2(1.5f * mult, 1.5f * mult),
        };

        var tris = Triangulate(segs, interiorPs);
        tris.ForEach(t => _client.DrawTri(t));
    }

    private List<Triangle> Triangulate(List<LineSegment> boundarySegs, 
        HashSet<Vector2> interiorPoints)
    {
        var boundaryPoints = boundarySegs.GetPoints().ToList();
        if(boundaryPoints.Last() == boundaryPoints[0]) boundaryPoints.RemoveAt(boundaryPoints.Count - 1);
        var boundaryHash = boundaryPoints.ToHashSet();
        
        var indices = new List<int>();
        var indexDic = new Dictionary<Vector2, int>();
        var boundaryPointPairs = new HashSet<V2Edge>();
        for (var i = 0; i < boundaryPoints.Count; i++)
        {
            var prev = (i - 1 + boundaryPoints.Count) % boundaryPoints.Count;
            var next = (i + 1) % boundaryPoints.Count;

            var a = boundaryPoints.Prev(i);
            var b = boundaryPoints[i];
            indexDic.Add(b, i);
            var c = boundaryPoints.Next(i);
            indices.Add(i);
            indices.Add(next);
            boundaryPointPairs.Add(new V2Edge(a, b));
            // boundaryPointPairs.Add(new V2Edge(a, c));
            boundaryPointPairs.Add(new V2Edge(b, c));
        }
        
        
        var allPoints = new List<Vector2>();
        allPoints.AddRange(boundaryPoints);
        // allPoints.AddRange(interiorPoints);
        var hash = allPoints.ToHashSet();
        
        var con = new ConstrainedPointSet(allPoints.GetPoly2TriTriPoints());
        Poly2Tri.P2T.Triangulate(con);
        var tris = new List<Triangle>();
        foreach (var dt in con.Triangles)
        {
            var t = dt.GetTri();
            bool draw = true;
            t.ForEachPoint(p =>
            {
                if (hash.Contains(p) == false)
                {
                    draw = false;
                    return;
                }
            });
            if (draw == true && t.AllPoints(boundaryHash.Contains))
            {
                if (t.AnyPointPairs((v, w) =>
                    {
                        var edge = new V2Edge(v, w);
                        if (boundaryPointPairs.Contains(edge) == false)
                        {
                            GD.Print("bad pair " + v + " " + w);
                            return true;
                        }
                        return false;
                    }))
                {
                    draw = false;
                }
            }
            if(draw) tris.Add(t);
        }

        return tris;
    }
}
