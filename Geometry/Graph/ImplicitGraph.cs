
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;


public class ImplicitGraph
{
    public static ImplicitGraph<TNode, TEdge> Get<TNode, TEdge>()
        where TNode : IGraphNode<TNode, TEdge>
    {
        return new ImplicitGraph<TNode, TEdge>(n => true, n => n.Neighbors, (n, m) => n.Neighbors.Contains(m),
            (n, m) => n.GetEdge(m));
    }
}
public class ImplicitGraph<TNode, TEdge> : IReadOnlyGraph<TNode, TEdge>
{
    private Func<TNode, bool> _contains;
    private Func<TNode, IReadOnlyCollection<TNode>> _getNeighbors;
    private Func<TNode, TNode, TEdge> _getEdge;
    private Func<TNode, TNode, bool> _hasEdge;
    
    [SerializationConstructor] public ImplicitGraph(Func<TNode, bool> contains, Func<TNode, IReadOnlyCollection<TNode>> getNeighbors, 
        Func<TNode, TNode, bool> hasEdge,
        Func<TNode, TNode, TEdge> getEdge)
    {
        _contains = contains;
        _getNeighbors = getNeighbors;
        _getEdge = getEdge;
        _hasEdge = hasEdge;
    }
    
    public bool HasEdge(TNode t1, TNode t2)
    {
        return _hasEdge(t1, t2);
    }

    public TEdge GetEdge(TNode from, TNode to)
    {
        return _getEdge(from, to);
    }

    public bool HasNode(TNode value)
    {
        return _contains(value);
    }

    public IReadOnlyCollection<TNode> GetNeighbors(TNode value)
    {
        return _getNeighbors(value);
    }
}
