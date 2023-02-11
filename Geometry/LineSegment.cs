using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


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

    public float DistanceTo(Vector2 pointRel)
    {
        return pointRel.DistToLine(From, To);
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
}

public static class LineSegmentExt
{
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
            var segBreaks = breakPoints.Where(p => seg.ContainsPoint(p)).OrderBy(b => b.DistanceTo(seg.From)).ToList();

            if (segBreaks.Count() > 0)
            {
                if (segBreaks.Count() > 1) throw new Exception();
                var breakPoint = segBreaks.First();
                var index = breakPoints.IndexOf(breakPoint);
                var width = widths[index];
                
                if (breakPoint == seg.From || breakPoint == seg.To) throw new Exception();
                if (seg.From.DistanceTo(breakPoint) <= width / 2f || seg.To.DistanceTo(breakPoint) <= width / 2f)
                {
                    throw new Exception();
                }
                
                var newFrom = seg.From + (breakPoint - seg.From).Shorten(width / 2f);
                var newTo = seg.To + (breakPoint - seg.To).Shorten(width / 2f);
                
                var before = new LineSegment(seg.From, newFrom);
                result.Add(before);
                var newSeg = new LineSegment(newFrom, newTo);
                result.Add(newSeg);
                inserted.Add(newSeg);
                var after = new LineSegment(newTo, seg.To);
                result.Add(after);
            }
            else result.Add(seg);
        }

        return result;
    }
}