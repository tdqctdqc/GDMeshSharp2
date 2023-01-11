using Godot;
using System;
using System.Collections.Generic;

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
                if(disturbedEdges.Contains(new Vector2(poly.Id, nPoly.Id))
                   || disturbedEdges.Contains(new Vector2(nPoly.Id, poly.Id)))
                {
                    continue;
                }

                disturbedEdges.Add(new Vector2(poly.Id, nPoly.Id));
                DisturbEdge(poly, nPoly, noise);
            }
        }
    }

    private static void DisturbEdge(Polygon poly1, Polygon poly2, OpenSimplexNoise noise)
    {
        if (poly1.Center.DistanceTo(poly2.Center) > 1000f)
        {
            //todo create clone to do it
            
            return;
        }
        var border = poly1.GetEdge(poly2);
        var stack = new Stack<EdgeDisturbInfo>();
        var disturbed = new List<EdgeDisturbInfo>();
        var newPoints = new List<Vector2>();
        var first = new EdgeDisturbInfo(poly1, poly2);
        first.Disturb(3, .3f, .2f, 5f, noise);
        var curr = first;
        while (curr != null || stack.Count > 0)
        {
            while (curr != null)
            {
                stack.Push(curr);
                curr = curr.Left;
            }

            curr = stack.Pop();
            disturbed.Add(curr);
            curr = curr.Right;
        }
        for (var i = 0; i < disturbed.Count; i++)
        {
            newPoints.Add(disturbed[i].Start);
        }
        newPoints.Add(disturbed[disturbed.Count - 1].End);
        border.ReplacePoints(newPoints);
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

        public EdgeDisturbInfo(Polygon p1, Polygon p2)
        {
            P1 = p1.Center;
            P2 = p2.Center;
            var edge = p1.GetEdge(p2).GetPointsAbs();
            Start = edge[0];
            End = edge[1];
            T1 = new Triangle(P1, Start, End);
            T2 = new Triangle(P2, Start, End);
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
            
            var t1left = new Triangle(P1, splitPoint, Start);
            var t2left = new Triangle(splitPoint, P2, Start);
            var left = new EdgeDisturbInfo(P1, P2, Start, splitPoint);
            var t1right = new Triangle(P1, splitPoint, End);
            var t2right = new Triangle(P2, splitPoint, End);
            var right = new EdgeDisturbInfo(P2, P1, splitPoint, End);
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
