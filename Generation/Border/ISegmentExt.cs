using System.Collections.Generic;
using System.Linq;
using Godot;

public static class ISegmentExt
{
    public static TSeg Reverse<TSeg, TPrim>(this TSeg s)
        where TSeg : ISegment<TPrim>
    {
        return (TSeg)s.ReverseGeneric();
    }
    public static bool ConnectsToStart<T>(this ISegment<T> seg, ISegment<T> connect)
    {
        return connect.To.Equals(seg.From);
    }

    public static bool ConnectsToEnd<T>(this ISegment<T> seg, ISegment<T> connect)
    {
        return connect.From.Equals(seg.To);
    }

    public static List<TSeg> OrderEndToStart<TSeg>
        (this IEnumerable<TSeg> segs) where TSeg : ISegment
    {
        var segCount = segs.Count();
        var segsSample = segs.ToList();
        var res = new List<TSeg>{segs.First()};
        segsSample.Remove(segs.First());
        //scan for next
        var currLast = res.Last();
        var next = segsSample.FirstOrDefault(s => s.From.Equals(currLast.To));
        while (next != null && res.Count < segCount)
        {
            res.Add(next);
            segsSample.Remove(next);
            currLast = next;
            next = segsSample.FirstOrDefault(s => s.From.Equals(currLast.To));
        }
        
        var currFirst = res[0];
        var prevRes = new List<TSeg>();
        var prev = segsSample.FirstOrDefault(s => s.To.Equals(currFirst.From));
        while (prev != null && prevRes.Count + res.Count < segCount)
        {
            prevRes.Add(prev);
            segsSample.Remove(prev);
            currFirst = prev;
            prev = segsSample.FirstOrDefault(s => s.To.Equals(currFirst.From));
        }

        prevRes.Reverse();
        prevRes.AddRange(res);
        if (prevRes.Count != segCount)
        {
            if (prevRes.IsContinuous()) return prevRes;
            GD.Print($"res is {prevRes.Count} segments source is {segCount}");
            // GD.Print($"degen count {segs.Where(s => s.From == s.To).Count()}");
            GD.Print("SOURCE");
            GD.Print(segs.Select(ls => ls.ToString()).ToArray());
            GD.Print("RESULT");
            GD.Print(prevRes.Select(ls => ls.ToString()).ToArray());
            
            // throw new SegmentsNotConnectedException(data, poly, segs, prevRes, null);
        }
        
        return prevRes;
    }
    
    
    public static bool IsContinuous<TSeg>(this IReadOnlyList<TSeg> segs)
        where TSeg : ISegment
    {
        for (var i = 0; i < segs.Count - 1; i++)
        {
            if (segs[i].To.Equals(segs[i + 1].From) == false) return false;
        }
        return true;
    }
    public static bool IsContinuous(this IReadOnlyList<ISegment> segs)
    {
        for (var i = 0; i < segs.Count - 1; i++)
        {
            if (segs[i].To.Equals(segs[i + 1].From) == false) return false;
        }
        return true;
    }
}
