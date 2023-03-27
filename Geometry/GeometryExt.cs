using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class GeometryExt
{
    

    public static float RadToDegrees(this float rad)
    {
        return (360f * rad / (Mathf.Pi * 2f));
    }

    

    

    
    
    public static List<Triangle> ToTriangles(this List<Vector2> triPoints)
    {
        var result = new List<Triangle>();
        for (var i = 0; i < triPoints.Count; i+=3)
        {
            result.Add(new Triangle(triPoints[i], triPoints[i + 1], triPoints[i + 2]));
        }
        return result;
    }

    public static List<Triangle> TriangulateSegment(this List<LineSegment> segs, 
        LineSegment startLeg, LineSegment endLeg)
    {
        if (startLeg.From != endLeg.To) throw new Exception();
        var anchor = startLeg.From;
        var tris = new List<Triangle>();

        var partitionIndices = new List<int>{};
        var partitionNewPoints = new List<List<Vector2>>();
        var startRay = startLeg.To - anchor;
        var startNewPoints = startLeg.To.GeneratePointsAlong(30f, 5f, false, null, anchor).Select(p => p.Intify()).ToList();
        var endNewPoints = endLeg.From.GeneratePointsAlong(30f, 5f, false, null, anchor).Select(p => p.Intify()).ToList();
        for (var i = 0; i < segs.Count; i++)
        {
            var seg = segs[i];

            var ray = seg.To - anchor;

            if (startRay.GetCCWAngleTo(ray) > Mathf.Pi)
            {
                partitionIndices.Add(i);
                var newPs = seg.To.GeneratePointsAlong(50f, 5f, false, null, anchor).Select(p => p.Intify()).ToList();;
                partitionNewPoints.Add(newPs);
                startRay = ray;
            }
        }


        if (partitionIndices.Count > 0)
        {
            var prev = 0;
            var prevPoints = startNewPoints;
            var start = startLeg.To;
            for (var i = 0; i < partitionIndices.Count; i++)
            {
                var partIndex = partitionIndices[i];
                
                var sliceSegs = segs.GetRange(prev, partIndex - prev);
                var end = segs[partIndex].From;
                var startSeg = new LineSegment(anchor, start);
                var endSeg = new LineSegment(end, anchor);

                sliceSegs.TriangulateConcaveSegment(startSeg, endSeg, tris, prevPoints, new List<Vector2>());
                prevPoints = partitionNewPoints[i];
                start = end;
                prev = partIndex;
            }

            var lastSeg = segs[partitionIndices.Last()];
            var lastStartSeg = new LineSegment(anchor, lastSeg.From);
            var lastSliceSegs = segs.GetRange(partitionIndices.Last(), segs.Count - partitionIndices.Last());
            var lastEndSeg = new LineSegment(lastSeg.To, anchor);
            lastSliceSegs.TriangulateConcaveSegment(lastStartSeg, lastEndSeg, tris, prevPoints, endNewPoints);
        }
        else
        {
            TriangulateConcaveSegment(segs, startLeg, endLeg, tris, startNewPoints, endNewPoints);
        }
        
        return tris;
    }

    public static void TriangulateConcaveSegment(this List<LineSegment> segs,
        LineSegment startLeg, LineSegment endLeg, List<Triangle> tris, List<Vector2> newStartPoints, 
        List<Vector2> newEndPoints)
    {
        
        var points = new List<Vector2>();
        segs.Add(startLeg);
        segs.Add(endLeg);
        points.AddRange(segs.GenerateInteriorPoints(50f, 10f));
        points.AddRange(segs.GetPoints());
        segs.ForEach(b => b.GeneratePointsAlong(50f, 10f, points));

        points.AddRange(newStartPoints);
        points.AddRange(newEndPoints);

        try
        {
            tris.AddRange(
                DelaunayTriangulator.TriangulatePoints(points.Distinct().Select(p => p.Intify()).ToList())
                    .Where(t => t.IsDegenerate() == false));
        }
        catch (Exception e)
        {
            // GD.Print(points.Distinct().Select(p => p.ToString()).ToArray());
            throw;
        }
        
    }
}
