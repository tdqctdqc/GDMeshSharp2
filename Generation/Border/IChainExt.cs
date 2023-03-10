using System;
using System.Collections.Generic;
using System.Linq;

public static class IChainExt
{

    public static IEnumerable<TSeg> UnionSegs<TChain, TSeg, TPrim>(this IEnumerable<TChain> borders)
        where TChain : IChain<TSeg, TPrim> where TSeg : ISegment<TPrim>
    {
        return borders.OrderEndToStart<TChain, TPrim>().SelectMany(b => b.Segments);
    }




}