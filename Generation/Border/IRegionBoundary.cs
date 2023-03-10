
    public interface IRegionBoundary<TNode> : IBoundary<TNode>
        where TNode : IGraphNode<TNode>
    {
        IRegion<TNode> Region { get; } 
    }
