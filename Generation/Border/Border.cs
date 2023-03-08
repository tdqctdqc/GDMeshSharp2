
using System;
using System.Collections.Generic;
using System.Linq;

public class Border<T> : IBorder<T>
{
    public IReadOnlyList<T> Elements => _elements;
    private List<T> _elements;
}

public static class BorderExt
{
    public static IEnumerable<TSub> GetBorderElements<TSub, TEdge>(this IReadOnlyGraph<TSub, TEdge> graph,
        IReadOnlyHash<TSub> els)
    {
        return els.Where(e => graph.GetNeighbors(e).Any(n => els.Contains(n) == false));
    }

    public static List<TEdge> GetOrderedEdges<TNode, TEdge>(this IReadOnlyGraph<TNode, TEdge> graph,
        IEnumerable<TNode> elements)
    {
        return GetOrderedBorderPairs(graph, elements).Select(e => graph.GetEdge(e.Native, e.Foreign)).ToList();
    }
    public static List<BorderEdge<TNode>> GetOrderedBorderPairs<TNode, TEdge>(this IReadOnlyGraph<TNode, TEdge> graph,
        IEnumerable<TNode> elements)
    {
        var hash = elements.ToHashSet().ReadOnly();
        var borderSubs = graph.GetBorderElements(hash);
        var nativeEdgeDic = new Dictionary<TNode, List<BorderEdge<TNode>>>();
        var foreignEdgeDic = new Dictionary<TNode, List<BorderEdge<TNode>>>();
        
        foreach (var sub in borderSubs)
        {
            if (nativeEdgeDic.ContainsKey(sub))
            {
                continue;
            }

            var thisSubEdges = graph.GetNeighbors(sub).Where(n => hash.Contains(n) == false)
                .Select(n => new BorderEdge<TNode>(sub, n)).ToList();
            nativeEdgeDic.Add(sub, thisSubEdges);
            foreach (var e in thisSubEdges)
            {
                foreignEdgeDic.AddOrUpdate(e.Foreign, e);
            }
        }
        
        var firstSub = borderSubs.First();
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
        bool adjacentEdge(BorderEdge<TNode> e1, BorderEdge<TNode> e2)
        {
            return adjacentSub(e1, e2.Native) || adjacentSub(e1, e2.Foreign);
        }
        bool adjacentSub(BorderEdge<TNode> e, TNode s)
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
