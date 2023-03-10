using System.Collections.Generic;
public interface IReadOnlyGraph<TNode> 
{
    bool HasEdge(TNode from, TNode to);
    bool HasNode(TNode value);
    IReadOnlyCollection<TNode> GetNeighbors(TNode value);
}
public interface IReadOnlyGraph<TNode, TEdge> : IReadOnlyGraph<TNode>
{
    TEdge GetEdge(TNode from, TNode to);
}