
using System;
using System.Collections.Generic;

public interface IContiguousRegion<TNode> : IRegion<TNode>
    where TNode : IGraphNode<TNode>
{
    Action<IEnumerable<IContiguousRegion<TNode>>> Split { get; set; }
    IRegionBoundary<TNode> Border { get; }
    IReadOnlyHash<IContiguousRegion<TNode>> K1Regions { get; }
}
