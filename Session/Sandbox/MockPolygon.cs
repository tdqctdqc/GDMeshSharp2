using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using Godot;

public class MockPolygon
{
    public int Id { get; private set; }
    public Vector2 Center { get; private set; }
    public List<LineSegment> BorderSegments { get; private set; }
    public PolyTerrainTris Tris { get; private set; }
    public MockPolygon(Vector2 center, List<LineSegment> borderSegs, List<Vector2> riverPoints, List<float> riverWidths,
        int id)
    {
        Center = center;
        Id = id;

        
        MakeTris(borderSegs, riverPoints, riverWidths);
        
        
        
        
        
    }
    
    private void MakeTris(List<LineSegment> borderSegsRel, List<Vector2> riverPointsRel, List<float> riverWidths)
    {
        var brokenSegs = borderSegsRel.InsertOnPoints(riverPointsRel, riverWidths, out var riverSegs);

        if (riverPointsRel.Count == 1)
        {
            RiverSource(borderSegsRel, riverSegs);
        }
        else if (riverPointsRel.Count > 1)
        {
            
        }
        else
        {
            
        }
    }

    private void RiverSource(List<LineSegment> borderSegsRel, HashSet<LineSegment> riverSegs)
    {
        var riverSeg = riverSegs.First();
        var index = borderSegsRel.IndexOf(riverSeg);
        var riverTri = new Triangle(riverSeg.From, riverSeg.To, Vector2.Zero);
        var fromLegNewPoints = Mathf.CeilToInt(riverSeg.From.Length() / Constants.PreferredMinPolyBorderSegLength);
        var toLegNewPoints = Mathf.CeilToInt(riverSeg.To.Length() / Constants.PreferredMinPolyBorderSegLength);
        var newBorderSegs = new List<LineSegment>();
        
        var fromStep = riverSeg.From / (fromLegNewPoints + 1);
        var prev = riverSeg.From;
        for (var i = 0; i < fromLegNewPoints; i++)
        {
            var newP = riverSeg.From - (i + 1) * fromStep; 
            newBorderSegs.Add(new LineSegment(prev,newP));
            prev = newP;
        }
        
        
        var toStep = riverSeg.To / (toLegNewPoints + 1);
        for (var i = 0; i < toLegNewPoints; i++)
        {
            var newP = (i + 1) * toStep; 
            newBorderSegs.Add(new LineSegment(prev, newP));
            prev = newP;
        }
        newBorderSegs.Add(new LineSegment(prev, riverSeg.To));
        
        for (var i = 0; i < borderSegsRel.Count - 1; i++)
        {
            var j = (i + index + 1) % borderSegsRel.Count;
            newBorderSegs.Add(borderSegsRel[j]);
        }

        var nonRiverOutline = newBorderSegs.Select(ls => ls.From)
            .Union(newBorderSegs.Select(ls => ls.To))
            .Distinct()
            .ToList();
        
        
        var nonRiverTris = DelaunayTriangulator.TriangulatePoints(nonRiverOutline);
        var tris = new List<Triangle>();

        
        for (var i = 0; i < nonRiverTris.Count; i+=3)
        {
            tris.Add(new Triangle(nonRiverTris[i], nonRiverTris[i + 1], nonRiverTris[i + 2]));
        }
        Tris = PolyTerrainTris.Construct(tris);
    }
}
