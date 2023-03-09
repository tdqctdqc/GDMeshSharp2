using System.Collections.Generic;

public interface IReadOnlyGraph<TNode, TEdge>
{
    bool HasEdge(TNode from, TNode to);
    TEdge GetEdge(TNode from, TNode to);
    bool HasNode(TNode value);
    IReadOnlyCollection<TNode> GetNeighbors(TNode value);
}