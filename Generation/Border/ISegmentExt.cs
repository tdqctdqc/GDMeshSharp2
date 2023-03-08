using System.Collections.Generic;
using System.Linq;
using Godot;

public static class ISegmentExt
{
    public static bool IsCircuit(this IReadOnlyList<ISegment> segs)
    {
        for (int i = 0; i < segs.Count - 1; i++)
        {
            if (segs[i].To.Equals(segs[i + 1].From) == false) return false;
        }
        if (segs[segs.Count - 1].To.Equals(segs[0].From) == false) return false;

        return true;
    }
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
    public static List<List<ISegment>> DecomposeIntoContinuous(this List<ISegment> segs)
    {
        if (segs.IsContinuous()) return new List<List<ISegment>> {segs.ToList()};
        var res = new List<List<ISegment>>();
        int iter = 1;
        int place = 0;
        while (iter < segs.Count)
        {
            int thisIter = 1;
            var hash = new HashSet<int>{place};
            var first = place;
            while (segs.Prev(first).To == segs[first].From)
            {
                first = (first - 1 + segs.Count) % segs.Count;
                if (hash.Contains(first)) break;
                hash.Add(first);
                iter++;
                thisIter++;
            }
            
            var last = place;
            while (segs.Next(last).From == segs[last].To)
            {
                last = (last + 1) % segs.Count;
                if (hash.Contains(last)) break;
                hash.Add(last);
                iter++;
                thisIter++;
                place = (last + 1) % segs.Count;
            }

            var thisRes = new List<ISegment>();
            for (var i = 0; i < thisIter; i++)
            {
                thisRes.Add(segs.Modulo(i + first));
            }
            res.Add(thisRes);
        }

        return res;
    }
}
