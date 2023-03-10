
public interface IStaticGraphNode<TNode, TEdge> : IStaticGraphNode<TNode>
{
    IReadOnlyGraph<TNode, TEdge> Graph { get; }
}
public interface IStaticGraphNode<TNode> 
{
    TNode Element { get; }
    IReadOnlyGraph<TNode> Graph { get; }
}