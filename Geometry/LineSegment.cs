using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Poly2Tri.Triangulation.Sets;
using Poly2Tri.Utility;


public class LineSegment
{
    public Vector2 From { get; set; }
    public Vector2 To { get; set; }
    public Vector2 Mid() => (From + To) / 2f;
    public LineSegment(Vector2 from, Vector2 to)
    {
        From = from;
        To = to;
    }
    
    public LineSegment GetReverse()
    {
        return new LineSegment(To, From);
    }

    public LineSegment ChangeOrigin(Vector2 oldOrigin, Vector2 newOrigin)
    {
        return new LineSegment(From + oldOrigin - newOrigin, To + oldOrigin - newOrigin);
    }

    public LineSegment Translate(Vector2 offset)
    {
        return new LineSegment(From + offset, To + offset);
    }

    public void Clamp(float mapWidth)
    {
        if (Mid().x > mapWidth / 2f)
        {
            From += Vector2.Left * mapWidth;
            To += Vector2.Left * mapWidth;
        }

        if (Mid().x < -mapWidth / 2f)
        {
            From += Vector2.Right * mapWidth;
            To += Vector2.Right * mapWidth;
        }
    }
    public bool PointsTo(LineSegment l)
    {
        return l.From == To;
    }

    public float DistanceTo(Vector2 point)
    {
        return point.DistToLine(From, To);
    }

    public float Length()
    {
        return From.DistanceTo(To);
    }

    public bool ContainsPoint(Vector2 p)
    {
        return (p - From).Normalized() == (To - p).Normalized();
    }

    public static List<LineSegment> GetBoxBorderSegments(Vector2 center, float dim, int pointsPerSide)
    {
        var tl = center + new Vector2(-dim / 2f, -dim / 2f);
        var tr = center + new Vector2(dim / 2f, -dim / 2f);
        var br = center + new Vector2(dim / 2f, dim / 2f);
        var bl = center + new Vector2(-dim / 2f, dim / 2f);
        var res = new List<LineSegment>();
        var step = dim / (pointsPerSide + 1); 
        for (var i = 0; i <= pointsPerSide; i++)
        {
            res.Add(new LineSegment(tl + i * step * Vector2.Right, tl + (i + 1) * step * Vector2.Right));
            res.Add(new LineSegment(tr + i * step * Vector2.Down, tr + (i + 1) * step * Vector2.Down));
            res.Add(new LineSegment(br + i * step * Vector2.Left, br + (i + 1) * step * Vector2.Left));
            res.Add(new LineSegment(bl + i * step * Vector2.Up, bl + (i + 1) * step * Vector2.Up));
        }

        return res;
    }

    public bool LeftOf(Vector2 point)
    {
        return (To.x - From.x)*(point.y - From.y) - (To.y - From.y)*(point.x - From.x) > 0;
    }
}

public static class LineSegmentExt
{
    public static IEnumerable<LineSegment> GetOpposing(this IEnumerable<LineSegment> segs, Vector2 offset)
    {
        return segs.Select(s => s.Translate(-offset).GetReverse()).Reverse();
    }
    public static IEnumerable<LineSegment> ChangeOrigin(this IEnumerable<LineSegment> segs, Vector2 oldOrigin, Vector2 newOrigin)
    {
        return segs.Select(l => l.ChangeOrigin(oldOrigin, newOrigin));
    }
    public static IEnumerable<LineSegment> ChangeOrigin(this IEnumerable<LineSegment> segs, 
        MapPolygon oldOrigin, MapPolygon newOrigin, Data data)
    {
        var offset = newOrigin.GetOffsetTo(oldOrigin, data);
        return segs.Select(l => new LineSegment(l.From + offset,
            l.To + offset));
    }

    public static void SplitToMinLength(this MapPolygonBorder border, float minLength, GenWriteKey key)
    {
        var newSegsAbs = new List<LineSegment>();
        var segs = border.GetSegsAbs();
        var offset = border.HighId.Ref().GetOffsetTo(border.LowId.Ref(), key.Data);
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
                    var interp = j / (numSplits + 1f) ;
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
        
        border.ReplacePoints(newSegsAbs, 
            key);
    }
    public static List<Triangle> Triangulate(this List<LineSegment> boundarySegs, 
        HashSet<Vector2> interiorPoints = null)
    {
        var boundaryPoints = boundarySegs.GetPoints().ToList();
        if(boundaryPoints.Last() == boundaryPoints[0]) boundaryPoints.RemoveAt(boundaryPoints.Count - 1);
        var boundaryHash = boundaryPoints.ToHashSet();
        
        var indices = new List<int>();
        var indexDic = new Dictionary<Vector2, int>();
        for (var i = 0; i < boundaryPoints.Count; i++)
        {
            var next = (i + 1) % boundaryPoints.Count;
            indexDic.Add(boundaryPoints[i], i);
            indices.Add(i);
            indices.Add(next);
        }
        
        if(interiorPoints != null) boundaryPoints.AddRange(interiorPoints);
        var hash = boundaryPoints.ToHashSet();
        
        var con = new ConstrainedPointSet(boundaryPoints.GetPoly2TriTriPoints());
        // con.WindingOrder = Point2DList.WindingOrderType.Clockwise;

        Poly2Tri.P2T.Triangulate(con);
        var tris = new List<Triangle>();
        foreach (var dt in con.Triangles)
        {
            var t = dt.GetTri();
            var centroid = t.GetCentroid();
            if (t.AnyPoint(p => hash.Contains(p) == false)) continue;
            
            if (t.AllPoints(boundaryHash.Contains))
            {
                var index = indexDic[t.A];
                var next = boundaryPoints
                    .FindNext(v => v == t.B || v == t.C, index);
                if (next != t.B) continue;
            }
            tris.Add(t);
        }

        return tris;
    }
    public static List<LineSegment> BreakOnPoints(this List<LineSegment> segs, 
        List<Vector2> breakPoints)
    {
        var result = new List<LineSegment>();
        for (var i = 0; i < segs.Count; i++)
        {
            var seg = segs[i];
            var segBreaks = breakPoints.Where(p => seg.ContainsPoint(p)).OrderBy(b => b.DistanceTo(seg.From)).ToList();

            if (segBreaks.Count() > 0)
            {
                if (segBreaks.Any(b => b == seg.From || b == seg.To)) throw new Exception();
                result.Add(new LineSegment(seg.From, segBreaks.First()));
                for (var j = 0; j < segBreaks.Count - 1; j++)
                {
                    result.Add(new LineSegment(segBreaks[j], segBreaks[j + 1]));
                }
                result.Add(new LineSegment(segBreaks.Last(), seg.To));
            }
            else result.Add(seg);
        }

        return result;
    }
    
    public static List<LineSegment> InsertOnPoints(this List<LineSegment> segs, 
        List<Vector2> breakPoints, List<float> widths, out HashSet<LineSegment> inserted)
    {
        var result = new List<LineSegment>();
        inserted = new HashSet<LineSegment>();
        for (var i = 0; i < segs.Count; i++)
        {
            var seg = segs[i];
            bool hasBreak = false;
            Vector2 breakPoint = Vector2.Zero;
            int breakPointIndex = -1;
            for (var j = 0; j < breakPoints.Count; j++)
            {
                if (seg.ContainsPoint(breakPoints[j]))
                {
                    hasBreak = true;
                    breakPoint = breakPoints.FirstOrDefault(p => seg.ContainsPoint(p));
                    breakPointIndex = j;
                    break;
                }
            }
            
            if (hasBreak)
            {
                var width = widths[breakPointIndex];
                
                if (breakPoint == seg.From || breakPoint == seg.To) throw new Exception();
                width = Mathf.Min(width, Mathf.Min(seg.From.DistanceTo(breakPoint) * 1.5f, seg.To.DistanceTo(breakPoint) * 1.5f));
                if (seg.From.DistanceTo(breakPoint) <= width / 2f || seg.To.DistanceTo(breakPoint) <= width / 2f)
                {
                    throw new Exception($"segment not big enough, width is {seg.Length()} and insert is ${width}");
                }
                
                var newFrom = seg.From + (breakPoint - seg.From).Shorten(width / 2f);
                var newTo = seg.To + (breakPoint - seg.To).Shorten(width / 2f);
                
                result.Add(new LineSegment(seg.From, newFrom));
                var newSeg = new LineSegment(newFrom, newTo);
                result.Add(newSeg);
                inserted.Add(newSeg);
                result.Add(new LineSegment(newTo, seg.To));
            }
            else result.Add(seg);
        }

        return result;
    }
    public static IEnumerable<LineSegment> GetLineSegments(this List<Vector2> points, bool close = false)
    {
        
        return Enumerable.Range(0, points.Count() - (close ? 0 : 1))
            .Select(i =>
            {
                return new LineSegment(points[i], points.Modulo(i + 1));
            });
    }

    public static LineSegment GetFirst(this IEnumerable<LineSegment> segments)
    {
        var noTo = segments.Where(s => segments.Any(n => n.PointsTo(s) == false));
        if (noTo.Count() != 1) throw new Exception();
        return segments.First(s => segments.Any(n => n.PointsTo(s) == false));
    }
    public static IEnumerable<Vector2> GetPoints(this IEnumerable<LineSegment> pairs)
    {
        var result = Enumerable.Range(0, pairs.Count())
            .Select(i => pairs.ElementAt(i).From)
            .ToList();
        result.Add(pairs.Last().To);
        return result;
    }
    public static Vector2 GetPointAtRatio(this IEnumerable<LineSegment> pairs, float ratio)
    {
        var totalLength = pairs.GetLength();
        var lengthSoFar = 0f;
        var iter = 0;
        var count = pairs.Count();
        while (iter < count)
        {
            var seg = pairs.ElementAt(iter);
            if (lengthSoFar + seg.Length() > totalLength * ratio)
            {
                var portion = totalLength * ratio - lengthSoFar;
                return seg.From.LinearInterpolate(seg.To, portion / seg.Length());
            }
            lengthSoFar += seg.Length();
            iter++;
        }

        throw new Exception();
    }
    
    public static Vector2 GetPointAtLength(this IEnumerable<LineSegment> pairs, float length)
    {
        var totalLength = pairs.GetLength();
        var lengthSoFar = 0f;
        var iter = 0;
        var count = pairs.Count();
        while (iter < count)
        {
            var seg = pairs.ElementAt(iter);
            if (lengthSoFar + seg.Length() > length)
            {
                var portion = totalLength - lengthSoFar;
                return seg.From.LinearInterpolate(seg.To, portion / seg.Length());
            }
            lengthSoFar += seg.Length();
            iter++;
        }

        throw new Exception();
    }

    public static float GetLength(this IEnumerable<LineSegment> pairs)
    {
        return pairs.Select(p => p.From.DistanceTo(p.To)).Sum();
    }
    public static Vector2 GetMiddlePoint(this IEnumerable<LineSegment> pairs)
    {
        var totalLength = pairs.GetLength();
        var lengthSoFar = 0f;
        var iter = 0;
        var count = pairs.Count();
        while (iter < count)
        {
            var seg = pairs.ElementAt(iter);
            if (lengthSoFar + seg.Length() > totalLength / 2f)
            {
                var portion = totalLength / 2f - lengthSoFar;
                return seg.From.LinearInterpolate(seg.To, portion / seg.Length());
            }
            lengthSoFar += seg.Length();
            iter++;
        }

        throw new Exception();
    }

}