using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using Godot;

public class MockPolygon
{
    public int Id { get; private set; }
    public List<LineSegment> BorderSegments { get; private set; }
    public PolyTerrainTris Tris { get; private set; }
    public MockPolygon(int id)
    {
        Id = id;
        BorderSegments = new List<LineSegment>();

        var dim = Vector2.One * 500f;
        var ps = PointsGenerator
            .GenerateConstrainedSemiRegularPoints(dim, 100f, 45f, true, true).Select(p => p - dim / 2f).ToList();
        
        var triPs = DelaunayTriangulator.TriangulatePoints(ps);
        var tris = new List<Triangle>();

        
        for (var i = 0; i < triPs.Count; i+=3)
        {
            tris.Add(new Triangle(triPs[i], triPs[i + 1], triPs[i + 2]));
        }
        Tris = PolyTerrainTris.Construct(tris);
    }
}
