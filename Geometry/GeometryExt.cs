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

    public static List<Vector2> StitchTogether(this List<List<Vector2>> segments)
    {
        var res = new List<Vector2>(segments[0]);
        for (int i = 1; i < segments.Count; i++)
        {
            var seg = segments[i];
            if (seg[0] == res[res.Count - 1])
            {
                for (var j = 1; j < seg.Count; j++)
                {
                    res.Add(seg[j]);
                }
            }
            else if (seg[seg.Count - 1] == res[res.Count - 1])
            {
                for (var j = seg.Count - 2; j >= 0; j--)
                {
                    res.Add(seg[j]);
                }
            }
            // else throw new Exception();
        }

        return res;
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
        var startNewPoints = startLeg.To.GeneratePointsAlong(50f, 5f, anchor);
        var endNewPoints = endLeg.From.GeneratePointsAlong(50f, 5f, anchor);
        var partitionNewPointsAll = new List<Vector2>();
        for (var i = 0; i < segs.Count; i++)
        {
            var seg = segs[i];

            var ray = seg.To - anchor;

            if (startRay.GetClockwiseAngleTo(ray) > Mathf.Pi)
            {
                partitionIndices.Add(i);
                var newPs = seg.To.GeneratePointsAlong(50f, 5f, anchor);
                partitionNewPoints.Add(newPs);
                partitionNewPointsAll.AddRange(newPs);
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
        
        partitionNewPointsAll.ForEach(p =>
        {
            var pTris = tris.Where(t => t.HasPoint(p));
            var avg = pTris.Select(t => (t.A + t.B + t.C) / 3).Avg();
            foreach (var pTri in pTris)
            {
                pTri.ReplacePoint(p, avg);
            }
        });
        return tris;
    }

    public static void TriangulateConcaveSegment(this List<LineSegment> segs,
        LineSegment startLeg, LineSegment endLeg, List<Triangle> tris, List<Vector2> newStartPoints, 
        List<Vector2> newEndPoints)
    {
        var segs2 = segs.Union(new List<LineSegment>(){
            startLeg
        }).Union(new List<LineSegment>{
            endLeg
        }).Distinct().ToList();
        var points = segs.GetPoints().ToList();
        
        points.AddRange(segs2.GenerateInteriorPoints(50f).Where(p => endLeg.DistanceTo(p) > 10f
                                                                    && startLeg.DistanceTo(p) > 10f
                                                                    && segs.Min(s => s.DistanceTo(p) > 10f)
        ));
        points.AddRange(newStartPoints);
        points.AddRange(newEndPoints);
        points.Add(startLeg.To);
        points.Add(startLeg.From);
        points.Add(endLeg.To);
        points.Add(endLeg.From);
        tris.AddRange(
            DelaunayTriangulator.TriangulatePoints(points.Distinct().ToList())
                .ToTriangles()
                .Where(t => t.IsDegenerate() == false)
            );
        
    }
}
