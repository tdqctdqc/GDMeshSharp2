using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        GD.Print(riverPoints.Count + " river points");
        
        MakeTris(borderSegs, riverPoints, riverWidths);
        
        
        
        
        
    }
    
    private void MakeTris(List<LineSegment> borderSegsRel, List<Vector2> riverPointsRel, List<float> riverWidths)
    {
        var brokenSegs = borderSegsRel.InsertOnPoints(riverPointsRel, riverWidths, out var riverSegs);

        if (riverPointsRel.Count == 1)
        {
            RiverSource(brokenSegs, riverSegs);
        }
        else if (riverPointsRel.Count > 1)
        {
            RiverJunction(brokenSegs, riverSegs);
        }
        else
        {
            
        }
    }

    private void RiverSource(List<LineSegment> borderSegsRel, HashSet<LineSegment> riverSegs)
    {
        var riverSeg = riverSegs.First();
        var riverSegIndex = borderSegsRel.IndexOf(riverSeg);

        var nonRiverSegs = new List<LineSegment>();
        for (var i = 1; i < borderSegsRel.Count; i++)
        {
            var index = (i + riverSegIndex) % borderSegsRel.Count;
            nonRiverSegs.Add(borderSegsRel[index]);
        }
        
        var riverTri = new Triangle(riverSeg.From, riverSeg.To, Vector2.Zero);

        var tris = new List<Triangle>();

        var segTris = nonRiverSegs.TriangulateSegment(
            new LineSegment(Center, riverSeg.To),
            new LineSegment(riverSeg.From, Center));
        tris.AddRange(segTris);
        tris.Add(riverTri);
        
        Tris = PolyTerrainTris.Construct(tris);
    }

    private void RiverJunction(List<LineSegment> borderSegsRel, HashSet<LineSegment> riverSegs)
    {
        GD.Print("constructing junction");
        GD.Print(riverSegs.Count + " river segs");
        var riverIndices = riverSegs
            .OrderByClockwise(Vector2.Zero, s => s.From, riverSegs.First())
            .Select(s => borderSegsRel.IndexOf(s)).ToList();
        var tris = new List<Triangle>();

        
        var junctionPoints = new List<Vector2>();
        for (var i = 0; i < riverIndices.Count; i++)
        {
            var riverIndex = riverIndices[i];
            var prevRiverIndex = riverIndices[(i + 1) % riverIndices.Count];
            var nextRiverIndex = riverIndices[(i - 1 + riverIndices.Count) % riverIndices.Count];
            
            var seg = borderSegsRel[riverIndex];
            var nextRSeg = borderSegsRel[nextRiverIndex];
            var prevRSeg = borderSegsRel[prevRiverIndex];

            var prevIntersect = GetIntersection(riverIndex, prevRiverIndex);
            var nextIntersect = GetIntersection(nextRiverIndex, riverIndex);
            junctionPoints.Add(nextIntersect);

            tris.Add(new Triangle(seg.From, prevIntersect, nextIntersect));
            tris.Add(new Triangle(seg.To, seg.From, nextIntersect));

            var thisNonRiverSegs = new List<LineSegment>();
            int iter = (riverIndex + 1) % borderSegsRel.Count;
            while (iter != nextRiverIndex)
            {
                thisNonRiverSegs.Add(borderSegsRel[iter]);
                iter++;
                iter = iter % borderSegsRel.Count;
            }
            var segTris = thisNonRiverSegs.TriangulateSegment(
                new LineSegment(nextIntersect, seg.To),
                new LineSegment(nextRSeg.From, nextIntersect)
                );
            
            tris.AddRange(segTris);
        }

        Vector2 GetIntersection(int fromIndex, int toIndex)
        {
            var to = borderSegsRel[fromIndex];
            var from = borderSegsRel[toIndex];
            if (to.Mid().Normalized() == -from.Mid().Normalized())
            {
                if (from.Mid() == Vector2.Zero) throw new Exception();
                if (to.Mid() == Vector2.Zero) throw new Exception();
                return to.From.LinearInterpolate(from.To,
                    from.Mid().Length() / (from.Mid().Length() + to.Mid().Length()));
            }
                
            var has = GeometryExt.GetLineIntersection(to.From, to.From - to.Mid(),
                from.To, from.To - from.Mid(), 
                out var intersect);
            if (Mathf.IsNaN(intersect.x) || Mathf.IsNaN(intersect.y)) throw new Exception();
            return intersect;
        }
          for (var i = 1; i < junctionPoints.Count - 1; i++)
          {
              var tri = new Triangle(junctionPoints[0], junctionPoints[i], junctionPoints[i + 1]);
              tris.Add(tri);
          }
        
        
        
        Tris = PolyTerrainTris.Construct(tris);
    }

}
