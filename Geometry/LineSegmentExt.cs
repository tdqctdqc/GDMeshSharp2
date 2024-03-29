using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


public static class LineSegmentExt
{

    public static List<LineSegment> FindTri(this List<LineSegment> segs)
    {
        for (var i = 0; i < segs.Count; i++)
        {
            var seg = segs[i];
            var pointsTo = segs.Where(s => seg.To == s.From);
            if (pointsTo.Count() == 0) continue;
            foreach (var cand in pointsTo)
            {
                var complete = segs.FirstOrDefault(s => s.To == seg.From && s.From == cand.To);
                if (complete is LineSegment ls) return new List<LineSegment> {seg, cand, complete};
            }
        }

        return null;
    }
    public static bool Intersects(this LineSegment ls, Vector2 point, Vector2 dir)
    {
        var intersect = Geometry.LineIntersectsLine2d(ls.From, ls.GetNormalizedAxis(), point, dir);
        if (intersect is Vector2 v == false) return false;
        var inX = (ls.From.x <= v.x && v.x <= ls.To.x) || (ls.From.x >= v.x && v.x >= ls.To.x);
        var inY = (ls.From.y <= v.y && v.y <= ls.To.y) || (ls.From.y >= v.y && v.y >= ls.To.y);
        return inX && inY;
    }
    public static Vector2 IntersectOrInf(this LineSegment ls, Vector2 point, Vector2 dir)
    {
        var intersect = Geometry.LineIntersectsLine2d(ls.From, ls.GetNormalizedAxis(), point, dir);
        if (intersect is Vector2 v == false) return Vector2.Inf;
        var inX = (ls.From.x <= v.x && v.x <= ls.To.x) || (ls.From.x >= v.x && v.x >= ls.To.x);
        var inY = (ls.From.y <= v.y && v.y <= ls.To.y) || (ls.From.y >= v.y && v.y >= ls.To.y);
        return inX && inY ? v : Vector2.Inf;
    }
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
            var e = new GeometryException("chainification could not complete");
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
    
    
    public static List<LineSegment> Chainify(this List<List<LineSegment>> chains)
    {
        var froms = new Dictionary<Vector2, List<LineSegment>>();
        var tos = new Dictionary<Vector2, List<LineSegment>>();
        Vector2 first = Vector2.Inf;
        for (var i = 0; i < chains.Count; i++)
        {
            var chain = chains[i];
            tos.Add(chain[chain.Count - 1].To, chain);
        }
        for (var i = 0; i < chains.Count; i++)
        {
            var chain = chains[i];
            froms.Add(chain[0].From, chain);
            if (tos.ContainsKey(chain[0].From) == false) first = chain[0].From;
        }

        if (first == Vector2.Inf) first = chains[0][0].From;

        var curr = froms[first];
        
        
        var res = new List<LineSegment>(curr);
        
        for (var i = 0; i < chains.Count - 1; i++)
        {
            var next = froms[curr[curr.Count - 1].To];
            res.AddRange(next);
            curr = next;
        }

        return res;
    }

    public static bool Neighboring(this MapPolygonEdge e, MapPolygonEdge n)
    {
        return e.HiNexus.Entity().IncidentEdges.Contains(n) || e.LoNexus.Entity().IncidentEdges.Contains(n);
    }

    public static void CompleteCircuit(this List<LineSegment> segs)
    {
        if(segs[segs.Count - 1].To != segs[0].From) segs.Add(new LineSegment(segs[segs.Count - 1].To, segs[0].From));
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
        edge.ReplaceMiddlePoints(newSegsAbs, key);
    }
    
    public static IEnumerable<LineSegment> GetLineSegments(this List<Vector2> points, bool close = false)
    {
        return Enumerable.Range(0, points.Count() - (close ? 0 : 1))
            .Select(i =>
            {
                return new LineSegment(points[i], points.Modulo(i + 1));
            });
    }

    public static List<Vector2> GetPoints(this IEnumerable<LineSegment> pairs)
    {
        var first = pairs.First();
        var result = pairs
            .Select(pair => pair.From)
            .ToList();
        var last = pairs.Last();
        if(last.To != first.From) result.Add(last.To);
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