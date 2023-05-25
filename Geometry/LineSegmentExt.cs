using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Poly2Tri.Triangulation;
using Poly2Tri.Triangulation.Sets;
using Poly2Tri.Utility;

public static class LineSegmentExt
{
    public static float GetAngleAroundSum(this List<LineSegment> segs, Vector2 center)
    {
        float res = 0f;
        for (var i = 0; i < segs.Count; i++)
        {
            var seg = segs[i];
            res += (seg.From - center).AngleTo(seg.To - center);
        }
        return res;
    }
    public static bool IsSame(this LineSegment ls, LineSegment compare)
    {
        if (ls == null && compare == null) return true;
        if (ls == null || compare == null) return false;
        return ls.From == compare.From && ls.To == compare.To;
    }
    public static Vector2 Average(this IEnumerable<LineSegment> segs)
    {
        var avgX = segs.Average(s => (s.From.x + s.To.x) / 2f);
        var avgY = segs.Average(s => (s.From.y + s.To.y) / 2f);
        var avg = new Vector2(avgX, avgY);
        return avg;
    }
    public static IEnumerable<LineSegment> GetInscribed(this IEnumerable<LineSegment> segs, Vector2 center,
        float insetFactor)
    {
        return segs.Select(s => new LineSegment((s.From - center) * insetFactor, (s.To - center) * insetFactor));
    }
    public static List<LineSegment> FlipChainify(this List<LineSegment> lineSegments)
    {
        var hash = new HashSet<LineSegment>(lineSegments);
        
        var start = hash.First();
        hash.Remove(start);
        var tos = new List<LineSegment>();
        var froms = new List<LineSegment>();
        var to = start.To;
        var from = start.From;

        // GD.Print("starting tos");
        while (hash.Count > 0)
        {
            var nextTo = hash.FirstOrDefault(ls => ls.From == to || ls.To == to);
            if (nextTo == null) break;
            hash.Remove(nextTo);
            if (nextTo.To == to)
            {
                nextTo = nextTo.Reverse();
            }
            to = nextTo.To;
            tos.Add(nextTo);
        }

        while (hash.Count > 0)
        {
            var nextFrom = hash.FirstOrDefault(ls => ls.From == from || ls.To == from);
            if (nextFrom == null) break;
            hash.Remove(nextFrom);
            if (nextFrom.From == from)
            {
                nextFrom = nextFrom.Reverse();
            }
            from = nextFrom.From;
            froms.Add(nextFrom);
        }

        froms.Reverse();
        froms.Add(start);
        froms.AddRange(tos);
        
        if (hash.Count != 0)
        {
            var e = new SegmentsException("chainification could not complete");
            e.AddSegLayer(lineSegments, "before");
            e.AddSegLayer(froms, "attempt");
            e.AddSegLayer(hash.ToList(), "leftover");
            
            throw e;
        }

        if (froms.IsChain() == false) throw new Exception();
        return froms; 
    }

    public static List<LineSegment> Chainify(this List<LineSegment> lineSegments)
    {
        var froms = new Dictionary<Vector2, LineSegment>();
        var tos = new Dictionary<Vector2, LineSegment>();
        Vector2 first = Vector2.Inf;
        for (var i = 0; i < lineSegments.Count; i++)
        {
            var seg = lineSegments[i];
            tos.Add(seg.To, seg);
        }
        for (var i = 0; i < lineSegments.Count; i++)
        {
            var seg = lineSegments[i];
            froms.Add(seg.From, seg);
            if (tos.ContainsKey(seg.From) == false) first = seg.From;
        }

        if (first == Vector2.Inf) first = lineSegments[0].From;

        var curr = froms[first];
        var res = new List<LineSegment>{curr};
        for (var i = 0; i < lineSegments.Count - 1; i++)
        {
            var next = froms[curr.To];
            res.Add(next);
            curr = next;
        }

        return res;
    }

    public static List<LineSegment> Circuitify(this List<List<LineSegment>> source)
    {
        var neighborSegs = source.Chainify();
        if (neighborSegs.IsCircuit() == false || neighborSegs.IsContinuous() == false)
        {
            var e = new SegmentsException("still not circuit");
            e.AddSegLayer(source.SelectMany(l => l).ToList(), "source");
            e.AddSegLayer(neighborSegs, "attempt");
            throw e;
        }
        return neighborSegs;
    }
    
    
    public static List<LineSegment> Chainify(this List<List<LineSegment>> source)
    {
        var sourceHash = source.ToHashSet();
        var startList = sourceHash.First();
        var from = startList.First().From;
        sourceHash.Remove(startList);
        var to = startList.Last().To;
        var wholeList = new List<List<LineSegment>>();
        while (sourceHash.FirstOrDefault(s => s.Last().To == from) is List<LineSegment> prevList)
        {
            sourceHash.Remove(prevList);
            from = prevList.First().From;
            wholeList.Insert(0, prevList);
        }
        wholeList.Add(startList);
        while (sourceHash.FirstOrDefault(s => s.First().From == to) is List<LineSegment> nextList)
        {
            sourceHash.Remove(nextList);
            to = nextList.Last().To;
            wholeList.Add(nextList);
        }
        
        var neighborSegs = wholeList.SelectMany(l => l).ToList();

        if (sourceHash.Count > 0)
        {
            var e = new SegmentsException("segments left over");
            e.AddSegLayer(source.SelectMany(l => l).ToList(), "source");
            e.AddSegLayer(neighborSegs, "attempt");
            e.AddSegLayer(sourceHash.SelectMany(l => l).ToList(), "leftover");
            throw e;
        }


        if (neighborSegs.IsContinuous() == false)
        {
            var e = new SegmentsException("still not circuit");
            e.AddSegLayer(source.SelectMany(l => l).ToList(), "source");
            e.AddSegLayer(neighborSegs, "attempt");
            throw e;
        }
        return neighborSegs;
    }
    public static List<LineSegment> Circuitify(this List<LineSegment> lineSegments)
    {
        var circuit = lineSegments.Chainify();
        if (circuit.First().From != circuit.Last().To)
        {
            circuit.Add(new LineSegment(circuit.Last().To, circuit.First().From));
        }
        if (circuit.IsCircuit() == false)
        {
            GD.Print("not circuit");
            for (var i = 0; i < circuit.Count; i++)
            {
                GD.Print(circuit[i].ToString());
            }
        }
        return circuit;
    }
    public static void SplitToMinLength(this MapPolygonEdge edge, float minLength, GenWriteKey key)
    {
        var newSegsAbs = new List<LineSegment>();
        var segs = edge.GetSegsAbs();
        var offset = edge.HighPoly.Entity().GetOffsetTo(edge.LowPoly.Entity(), key.Data);
        for (var i = 0; i < segs.Count; i++)
        {
            var seg = segs[i];
            var axis = (seg.To - seg.From);
            var l = seg.Length();
            if (l > minLength * 2f)
            {
                var numSplits = Mathf.FloorToInt(l / minLength) - 1;
                var prev = seg.From;
                for (int j = 1; j <= numSplits; j++)
                {
                    var interp = j / (numSplits + 1f);
                    var splitP = (seg.From + axis * interp).Intify();

                    newSegsAbs.Add(new LineSegment(prev, splitP));
                    prev = splitP;
                }

                newSegsAbs.Add(new LineSegment(prev, seg.To));
            }
            else
            {
                newSegsAbs.Add(seg);
            }
        }
        edge.ReplacePoints(newSegsAbs, key);
    }
    public static bool IsConvexAround(this List<LineSegment> segs, Vector2 center)
    {
        //todo implement
        return true;
    }
    
    public static List<PolyTri> PolyTriangulate(this IReadOnlyList<LineSegment> boundarySegs, GenData data, 
        MapPolygon poly, IdDispenser id,
        Graph<PolyTri, bool> graph,
        HashSet<Vector2> interiorPoints = null)
    {
        return boundarySegs.Triangulate(data, poly,
            (v, w, x) =>
            {
                var lf = data.Models.Landforms.GetAtPoint(poly, (v + w + x) / 3f, data);
                var vg = data.Models.Vegetation.GetAtPoint(poly, (v + w + x) / 3f, lf, data);
                return PolyTri.Construct(v, w, x, lf.MakeRef(), vg.MakeRef());
            }, 
            interiorPoints
        );
    }
    private static List<T> Triangulate<T>(this IReadOnlyList<LineSegment> boundarySegs, GenData data, MapPolygon poly,
        Func<Vector2, Vector2, Vector2, T> constructor, 
        HashSet<Vector2> interiorPoints = null) where T : Triangle
    {
        if (boundarySegs.IsCircuit() == false)
        {
            GD.Print("not circuit");
            throw new BadTriangulationException(poly, new List<Triangle>(), new List<Color>(), data, boundarySegs.ToList());
        }
        
        var points = boundarySegs.GetPoints().GetPoly2TriTriPoints();
        var boundaryHash = points.Select(p => p.GetV2()).ToHashSet();
        var hash = points.Select(p => p.GetV2()).ToHashSet();
        
        if(points.Last().GetV2() == points[0].GetV2()) points.RemoveAt(points.Count - 1);
        
        var constraints = new List<TriangulationConstraint>();
        for (var i = 0; i < points.Count; i++)
        {
            constraints.Add(new TriangulationConstraint(points.Next(i), points[i]));
        } 
        if(interiorPoints != null)
        {
            points.AddRange(interiorPoints.Select(p => p.GetPoly2TriTriPoint()));
            hash.AddRange(interiorPoints);
        }
        var con = new ConstrainedPointSet(points, constraints);
        Poly2Tri.P2T.Triangulate(con);

        var tris = new List<T>();
        for (var i = 0; i < con.Triangles.Count; i++)
        {
            var dt = con.Triangles[i];
            var t0 = dt.Points.Item0;
            var t1 = dt.Points.Item1;
            var t2 = dt.Points.Item2;
            var v0 = t0.GetV2();
            var v1 = t1.GetV2();
            var v2 = t2.GetV2();
            
            if (dt.Points.Any(p => hash.Contains(p.GetV2()) == false)) continue;

            if (boundaryHash.Contains(v0)
                && boundaryHash.Contains(v1)
                && boundaryHash.Contains(v2)
                )
            {
                var index = points.IndexOf(t0);    
                var next = points
                    .FindNext(v => v.EqualsV2(v1) || v.EqualsV2(v2), index);
                if (next.EqualsV2(v1) == false) continue;
            }

            var t = constructor(v0, v1, v2);
            tris.Add(t);
        }
        //todo check for missing
        return tris;
    }
    
    public static IEnumerable<LineSegment> GetLineSegments(this List<Vector2> points, bool close = false)
    {
        return Enumerable.Range(0, points.Count() - (close ? 0 : 1))
            .Select(i =>
            {
                return new LineSegment(points[i], points.Modulo(i + 1));
            });
    }

    public static IEnumerable<Vector2> GetPoints(this IEnumerable<LineSegment> pairs)
    {
        var result = Enumerable.Range(0, pairs.Count())
            .Select(i => pairs.ElementAt(i).From)
            .ToList();
        result.Add(pairs.Last().To);
        return result;
    }
    public static float GetLength(this IEnumerable<LineSegment> pairs)
    {
        return pairs.Select(p => p.From.DistanceTo(p.To)).Sum();
    }
    
    public static Vector2 GetCornerPoint(this LineSegment l1, LineSegment l2, float thickness)
    {
        var angle = l1.AngleBetween(l2);
        var axis = l1.GetNormalizedAxis();
        var perp = -axis.Perpendicular() * thickness;
        if (angle > Mathf.Pi) angle = -angle;
        var d = thickness / Mathf.Tan(angle / 2f);
        var p = l1.To - axis * d + perp; 

        return p;
    }
    public static float AngleBetween(this LineSegment l1, LineSegment l2)
    {
        if (l1.To != l2.From) throw new Exception();
        return (l1.From - l1.To).GetClockwiseAngleTo(l2.To - l2.From);
    }
}