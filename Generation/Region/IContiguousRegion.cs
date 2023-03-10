
using System.Collections.Generic;

public interface IContiguousRegion<TNode> : IRegion<TNode>
    where TNode : IStaticGraphNode<TNode>
{
    IBoundary<TNode> Border { get; }
    IReadOnlyHash<IContiguousRegion<TNode>> K1Regions { get; }
}
