
using System;
using System.Collections.Generic;
using System.Linq;

public class Chain<TSeg, TPrim> : IChain<TSeg>, ISegment<TSeg> 
    where TSeg : ISegment<TPrim>
{
    public TSeg this[int i] => Segments[i];
    public List<TSeg> Segments { get; private set; }
    public static Chain<TSeg, TPrim> Construct(List<TSeg> elements)
    {
        return new Chain<TSeg, TPrim>(elements.OrderEndToStart());
    }
    protected Chain(List<TSeg> segments)
    {
        Segments = segments;
    }


    ISegment<TSeg> ISegment<TSeg>.ReverseGeneric()
    {
        throw new NotImplementedException();
    }

    public ISegment<TSeg> ReverseGeneric()
    {
        var r = Segments.Select(e => e.Reverse<TSeg, TPrim>()).ToList();
        r.Reverse();
        return new Chain<TSeg, TPrim>(r);
    }

    IReadOnlyList<TSeg> IChain<TSeg>.Elements => Segments;

    TSeg ISegment<TSeg>.From => Segments[0];

    TSeg ISegment<TSeg>.To => Segments[Segments.Count - 1];
    bool ISegment.PointsTo(ISegment s)
    {
        if (s is ISegment<TSeg> t == false) return false;
        return Segments[Segments.Count - 1].To.Equals(t.From.From);
    }

    bool ISegment.ComesFrom(ISegment s)
    {
        if (s is ISegment<TSeg> t == false) return false;
        return Segments[0].From.Equals(t.To.To);
    }

    public int Count => Segments.Count;
}
