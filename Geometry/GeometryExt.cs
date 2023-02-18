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

    public static List<LineSegment> StitchTogether(this List<LineSegment> segs)
    {
        
        for (var i = 0; i < segs.Count - 1; i++)
        {
            if (segs[i].To != segs[i + 1].From) break;
            if (i == segs.Count - 1)
            {
                return segs.ToList();
            }
        }
        
        
        var origSeg = segs.ToList();
        var segments = segs.ToList();
        var count = segments.Count;
        var partials = new List<List<LineSegment>>();


        while (segments.Count > 0)
        {
            var partial = new List<LineSegment>{segments.First()};
            partials.Add(partial);
            segments.Remove(segments.First());
            
            var curr = partial[0];
            
            var next = segments.FirstOrDefault(s => s.From == curr.To);
            while (next != null)
            {
                partial.Add(next);
                curr = next;
                segments.Remove(next);
                next = segments.FirstOrDefault(s => s.From == curr.To);
            }


            curr = partial.First();
            var prev = segments.FirstOrDefault(s => s.To == curr.From);
            while (prev != null)
            {
                partial.Insert(0, prev);
                curr = prev;
                segments.Remove(prev);
                prev = segments.FirstOrDefault(s => s.To == curr.From);
            }
        }

        var res = new List<LineSegment>(partials[0]);
        
        for (var i = 1; i < partials.Count; i++)
        {
            var last = partials[i - 1].Last().To;
            var first = partials[0][0].From;
            var partial = partials[i];
            if (partial[0].From == last)
            {
                res.AddRange(partial);
            }
            else if(partial.Last().To == last)
            {
                res.AddRange(partial.Select(p => p.GetReverse()).Reverse());
            }
            else if (partial.First().From == first)
            {
                res.InsertRange(0, partial.Select(p => p.GetReverse()).Reverse());
            }
        }
        
        

        if (res.Count != count)
        {
            GD.Print($"avg x {segs.Average(s => s.From.x)}");
            // GD.Print($"{segments.Count} left");
            //
            // GD.Print("ORIG");
            // for (var i = 0; i < origSeg.Count; i++)
            // {
            //     GD.Print($"orig from {origSeg[i].From} to {origSeg[i].To}");
            // }
            //
            // for (var i = 0; i < segments.Count; i++)
            // {
            //     GD.Print($"remains from {segments[i].From} to {segments[i].To}");
            // }
            // for (var i = 0; i < res.Count; i++)
            // {
            //     GD.Print($"from {res[i].From} to {res[i].To}");
            // }

            throw new Exception("segments are not connected");
        }
        
        for (var i = 0; i < res.Count - 1; i++)
        {
            if (res[i].To != res[i + 1].From) throw new Exception();
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
        var startNewPoints = startLeg.To.GeneratePointsAlong(30f, 5f, false, null, anchor).Select(p => p.Intify()).ToList();
        var endNewPoints = endLeg.From.GeneratePointsAlong(30f, 5f, false, null, anchor).Select(p => p.Intify()).ToList();
        for (var i = 0; i < segs.Count; i++)
        {
            var seg = segs[i];

            var ray = seg.To - anchor;

            if (startRay.GetClockwiseAngleTo(ray) > Mathf.Pi)
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
            // throw;
        }
        
    }
}
