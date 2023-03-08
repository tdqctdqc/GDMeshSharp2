
using System;
using System.Collections.Generic;
using System.Linq;

public class Border<TBorderPrimitive, TRegion> : IBorder<TBorderPrimitive, TRegion>, ISegment<TBorderPrimitive> 
    where TBorderPrimitive : ISegment<TBorderPrimitive>
{
    public TRegion Native { get; private set; }
    public TRegion Foreign { get; private set; }
    public List<ISegment<TBorderPrimitive>> Elements { get; private set; }
    IReadOnlyList<ISegment<TBorderPrimitive>> IBorder<TBorderPrimitive, TRegion>.Elements => Elements;
    TBorderPrimitive ISegment<TBorderPrimitive>.From => Elements[0].From;
    TBorderPrimitive ISegment<TBorderPrimitive>.To => Elements[Elements.Count - 1].To;

    public Border(List<ISegment<TBorderPrimitive>> elements)
    {
        Elements = elements;
    }
    
    public ISegment<TBorderPrimitive> Reverse()
    {
        var r = Elements.Select(e => e.Reverse()).ToList();
        r.Reverse();
        return new Border<TBorderPrimitive, TRegion>(r);
    }
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
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    public static List<TEdge> GetOrderedEdges<TNode, TEdge, TSegment>
        (this IReadOnlyGraph<TNode, TEdge> graph, IEnumerable<TNode> elements)
        where TEdge : IBorder<TSegment, TNode>
    {
        var nativeHash = elements.ToHashSet().ReadOnly();
        var borderNodes = graph.GetBorderElements(nativeHash);
        var nativeEdgeDic = new Dictionary<TNode, List<TEdge>>();
        var foreignEdgeDic = new Dictionary<TNode, List<TEdge>>();
        
        foreach (var borderNode in borderNodes)
        {
            if (nativeEdgeDic.ContainsKey(borderNode))
            {
                continue;
            }

            var thisNodeEdges = graph.GetNeighbors(borderNode)
                .Where(foreign)
                .Select(foreignNode => graph.GetEdge(borderNode, foreignNode)).ToList();
            nativeEdgeDic.Add(borderNode, thisNodeEdges);
            foreach (var e in thisNodeEdges)
            {
                foreignEdgeDic.AddOrUpdate(getForeign(e), e);
            }
        }
        
        var firstSub = borderNodes.First();
        var firstEdges = nativeEdgeDic[firstSub];
        if (firstEdges.Count == 0) return null;
        var firstEdge = firstEdges[0];
        var firstEdgeNeighbors = getAdjEdges(firstEdge);
        
        var left = new List<TEdge>();
        var right = new List<TEdge>();
        var count = firstEdgeNeighbors.Count();
        var covered = new HashSet<TEdge> {firstEdge};
        
        if (count > 0)
        {
            traverse(firstEdgeNeighbors.ElementAt(0), left);
        }
        if (count > 1)
        {
            traverse(firstEdgeNeighbors.ElementAt(1), right);
        }

        var result = new List<TEdge>();
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
        
        IEnumerable<TEdge> getAdjEdges(TEdge edge)
        {
            return nativeEdgeDic[getNative(edge)].Union(foreignEdgeDic[getForeign(edge)])
                .Where(e => e.Equals(edge) == false && adjacentEdge(e, edge))
                .Distinct();
        }
        bool foreign(TNode node)
        {
            return nativeHash.Contains(node) == false;
        }
        TNode getForeign(TEdge e)
        {
            if (foreign(e.Native))
            {
                if (foreign(e.Foreign)) throw new Exception();
                return e.Native;
            }
            if (foreign(e.Foreign)) return e.Foreign;
            throw new Exception();
        }
        TNode getNative(TEdge e)
        {
            if (foreign(e.Native) == false)
            {
                if (foreign(e.Foreign) == false) throw new Exception();
                return e.Native;
            }
            if (foreign(e.Foreign) == false) return e.Foreign;
            throw new Exception();
        }
        bool adjacentEdge(TEdge e1, TEdge e2)
        {
            return adjacentNode(e1, getNative(e2)) || adjacentNode(e1, getForeign(e2));
        }
        bool adjacentNode(TEdge e, TNode s)
        {
            var ns = graph.GetNeighbors(s);
            return ns.Contains(getForeign(e)) && ns.Contains(getNative(e));
        }
        void traverse(TEdge e, List<TEdge> list)
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
