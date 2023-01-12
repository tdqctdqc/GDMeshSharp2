using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class EdgeDisturber
{
    public static void DisturbEdges(IReadOnlyList<Polygon> polys, Vector2 dimensions)
    {
        var noise = new OpenSimplexNoise();
        noise.Period = dimensions.x;
        noise.Octaves = 2;
        var disturbedEdges = new HashSet<Vector2>();
        for (var i = 0; i < polys.Count; i++)
        {
            var poly = polys[i];
            for (var j = 0; j < poly.Neighbors.Count; j++)
            {
                var nPoly = poly.Neighbors[j];
                if (poly.Id > nPoly.Id)
                {
                    DisturbEdge(poly, nPoly, noise);
                }
            }
        }
    }

    private static void DisturbEdge(Polygon highId, Polygon lowId, OpenSimplexNoise noise)
    {
        if (lowId.Center.DistanceTo(highId.Center) > 1000f) return;
        var border = highId.GetEdge(lowId);
        
        var hiSegs = border.HighSegsRel;
        var loSegs = border.LowSegsRel;

        var axis = border.GetOffsetToOtherPoly(highId);
        var dist = axis.Length();
        
        var deviation = Root.Random.RandfRange(.3f, .7f);
        var newHiPoint = axis * deviation;
        var newLoPoint = -axis * (1f - deviation);
        if (highId.Id % 100 == 0) GD.Print(newHiPoint.DistanceTo(hiSegs[0].From));
        var newSegsHi = new List<LineSegment>
            {new LineSegment(hiSegs[0].From, newHiPoint), 
                new LineSegment(newHiPoint, hiSegs[0].To)};
        var newSegsLow = new List<LineSegment>
            {new LineSegment(loSegs[0].From, newLoPoint), 
                new LineSegment(newLoPoint, loSegs[0].To)};
        border.ReplacePoints(newSegsHi, newSegsLow);
    }

    private class EdgeDisturbInfo
    {
        public Triangle T1 { get; private set; }
        public Triangle T2 { get; private set; }
        public Vector2 Start { get; private set; }
        public Vector2 End { get; private set; }
        public Vector2 P1 { get; private set; }
        public Vector2 P2 { get; private set; }
        public EdgeDisturbInfo Left { get; private set; }
        public EdgeDisturbInfo Right { get; private set; }
        
        public EdgeDisturbInfo(Vector2 p1, Vector2 p2, Vector2 edgeP1, Vector2 edgeP2)
        {
            P1 = p1;
            P2 = p2;
            T1 = new Triangle(p1, edgeP1, edgeP2);
            T2 = new Triangle(p2, edgeP1, edgeP2);
            Start = edgeP1;
            End = edgeP2;
        }


        public void Disturb(int times, float disturb, float disturbDecay, float minWidth, OpenSimplexNoise noise)
        {
            if (times == 0 || disturb < .1f || Check(minWidth) == false) return;
            var mid = (P1 + P2) / 2f;
            var sample = noise.GetNoise2d(mid.x, mid.y) * disturb * .5f;
            sample *= sample;
            var splitPoint1 = T1.GetRandomPointInside(sample, 1f - sample);
            var splitPoint2 = T2.GetRandomPointInside(sample, 1f - sample);
            var splitPoint = Root.Random.GetWeighted(splitPoint1, T1.GetArea(), splitPoint2, T2.GetArea());
            
            var left = new EdgeDisturbInfo(P1, P2, Start, splitPoint);
            var right = new EdgeDisturbInfo(P1, P2, End, splitPoint);
            if(left.Check(minWidth) && right.Check(minWidth))
            {
                Left = left;
                Left.Disturb(times - 1, disturb * disturbDecay, disturbDecay, minWidth, noise);

                Right = right;
                Right.Disturb(times - 1, disturb * disturbDecay, disturbDecay, minWidth, noise);
            }
        }

        private bool Check(float minWidth)
        {
            return (T1.BadTri(minWidth) || T2.BadTri(minWidth)) == false;
        }
    }
}
