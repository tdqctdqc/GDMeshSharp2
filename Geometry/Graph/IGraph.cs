
using System.Collections.Generic;

public interface IGraph<TNode, TEdge>
    : IReadOnlyGraph<TNode, TEdge>
{
    void SetEdgeValue(TNode t1, TNode t2, TEdge newEdgeVal);
    void AddEdge(TNode t1, TNode t2, TEdge edge);
    void AddNode(TNode element);
    bool Remove(TNode value);
}

public interface IReadOnlyGraph<TNode, TEdge>
{
    bool HasEdge(TNode from, TNode to);
    TEdge GetEdge(TNode from, TNode to);
    bool HasNode(TNode value);
    IReadOnlyCollection<TNode> GetNeighbors(TNode value);
    
}

public static class GraphExt
{
    
}