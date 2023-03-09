using System;
using System.Collections.Generic;
using System.Linq;

public static class IChainExt
{
    public static IEnumerable<TSub> GetBorderElements<TSub, TEdge>(this IReadOnlyGraph<TSub, TEdge> graph,
        IReadOnlyHash<TSub> els)
    {
        return els.Where(e => graph.GetNeighbors(e).Any(n => els.Contains(n) == false));
    }
    public static IEnumerable<TSeg> UnionSegs<TBorder, TSeg>(this IEnumerable<TBorder> borders)
        where TBorder : IChain<TSeg>, ISegment
    {
        return borders.OrderEndToStart().SelectMany(b => b.Elements);
    }
    public static TBorder UnionBorder<TBorder, TSeg>(Func<IEnumerable<TSeg>, TBorder> construct, 
        params TBorder[] borders)
        where TBorder : IChain<TSeg>, ISegment<TSeg>
    {
        var ordered = borders.OrderEndToStart().SelectMany(b => b.Elements);
        return construct(ordered);
    }
    
    
    
    public static List<BorderEdge<TNode>> GetOrderedBorderPairs<TNode, TEdge>(this IReadOnlyGraph<TNode, TEdge> graph,
        IEnumerable<TNode> elements)
    {
        var nativeHash = elements.ToHashSet().ReadOnly();
        var borderNodes = graph.GetBorderElements(nativeHash);
        var nativeEdgeDic = new Dictionary<TNode, List<BorderEdge<TNode>>>();
        var foreignEdgeDic = new Dictionary<TNode, List<BorderEdge<TNode>>>();
        
        foreach (var borderNode in borderNodes)
        {
            if (nativeEdgeDic.ContainsKey(borderNode))
            {
                continue;
            }

            var thisNodeEdges = graph.GetNeighbors(borderNode)
                .Where(foreign)
                .Select(foreignNode => new BorderEdge<TNode>(borderNode, foreignNode)).ToList();
            nativeEdgeDic.Add(borderNode, thisNodeEdges);
            foreach (var e in thisNodeEdges)
            {
                foreignEdgeDic.AddOrUpdate(e.Foreign, e);
            }
        }
        
        var firstSub = borderNodes.First();
        var firstEdges = nativeEdgeDic[firstSub];
        if (firstEdges.Count == 0) return null;
        var firstEdge = firstEdges[0];
        var firstEdgeNeighbors = getAdjEdges(firstEdge);
        
        var left = new List<BorderEdge<TNode>>();
        var right = new List<BorderEdge<TNode>>();
        var count = firstEdgeNeighbors.Count();
        var covered = new HashSet<BorderEdge<TNode>> {firstEdge};
        
        if (count > 0)
        {
            traverse(firstEdgeNeighbors.ElementAt(0), left);
        }
        if (count > 1)
        {
            traverse(firstEdgeNeighbors.ElementAt(1), right);
        }

        var result = new List<BorderEdge<TNode>>();
        for (int i = left.Count - 1; i >= 0; i--)
        {
            result.Add(left[i]);
        }
        result.Add(firstEdge);
        for (var i = 0; i < right.Count; i++)
        {
            result.Add(right[i]);
        }
        return result;
        
        IEnumerable<BorderEdge<TNode>> getAdjEdges(BorderEdge<TNode> edge)
        {
            return nativeEdgeDic[edge.Native].Union(foreignEdgeDic[edge.Foreign])
                .Where(e => e.Equals(edge) == false && adjacentEdge(e, edge))
                .Distinct();
        }

        bool foreign(TNode node)
        {
            return nativeHash.Contains(node) == false;
        }
        bool adjacentEdge(BorderEdge<TNode> e1, BorderEdge<TNode> e2)
        {
            return adjacentNode(e1, e2.Native) || adjacentNode(e1, e2.Foreign);
        }
        bool adjacentNode(BorderEdge<TNode> e, TNode s)
        {
            var ns = graph.GetNeighbors(s);
            return ns.Contains(e.Foreign) && ns.Contains(e.Native);
        }
        void traverse(BorderEdge<TNode> e, List<BorderEdge<TNode>> list)
        {
            list.Add(e);
            covered.Add(e);
            var adj = getAdjEdges(e).Where(a => covered.Contains(a) == false);
            if (adj.Count() > 1) throw new Exception();
            if (adj.Count() > 0)
            {
                traverse(adj.First(), list);
            }
        }
    }
    
    
}