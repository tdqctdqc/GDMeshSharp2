using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class ISegmentExt
{
    public static bool IsCircuit<TPrim>(this IReadOnlyList<ISegment<TPrim>> segs)
    {
        for (int i = 0; i < segs.Count - 1; i++)
        {
            if (segs[i].PointsTo(segs[i + 1]) == false) return false;
        }
        if (segs[segs.Count - 1].PointsTo(segs[0]) == false) return false;

        return true;
    }
    public static bool IsChain<TPrim>(this IReadOnlyList<ISegment<TPrim>> segs)
    {
        for (int i = 0; i < segs.Count - 1; i++)
        {
            if (segs[i].PointsTo(segs[i + 1]) == false) return false;
        }

        return true;
    }
    public static TSeg Reverse<TSeg, TPrim>(this TSeg s)
        where TSeg : ISegment<TPrim>
    {
        return (TSeg)s.ReverseGeneric();
    }

    public static List<TSeg> Ordered<TSeg, TPrim>
        (this IEnumerable<TSeg> segs) where TSeg : ISegment<TPrim>
    {
        //todo make it sort existing list so dont need tseg
        var segCount = segs.Count();
        var segsSample = segs.ToList();
        var res = new List<TSeg>{segs.First()};
        segsSample.Remove(segs.First());
        //scan for next
        var currLast = res.Last();
        var next = segsSample.FirstOrDefault(s => s.ComesFrom(currLast));
        while (next != null && res.Count < segCount)
        {
            res.Add(next);
            segsSample.Remove(next);
            currLast = next;
            next = segsSample.FirstOrDefault(s => s.ComesFrom(currLast));
        }
        
        var currFirst = res[0];
        var prevRes = new List<TSeg>();
        var prev = segsSample.FirstOrDefault(s => s.PointsTo(currFirst));
        while (prev != null && prevRes.Count + res.Count < segCount)
        {
            prevRes.Add(prev);
            segsSample.Remove(prev);
            currFirst = prev;
            prev = segsSample.FirstOrDefault(s => s.PointsTo(currFirst));
        }

        prevRes.Reverse();
        prevRes.AddRange(res);
        
        return prevRes;
    }
    
    
    public static bool IsContinuous<TPrim>(this IReadOnlyList<ISegment<TPrim>> segs)
    {
        for (var i = 0; i < segs.Count - 1; i++)
        {
            if (segs[i].PointsTo(segs[i + 1]) == false) return false;
        }
        return true;
    }
    
}
