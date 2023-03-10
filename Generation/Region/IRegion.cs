
using System;
using System.Collections.Generic;
using System.Linq;

public interface IRegion<TNode>
    where TNode : IGraphNode<TNode>
{
    IReadOnlyGraph<TNode> Graph { get; }
    IReadOnlyHash<TNode> Elements { get; }
    IReadOnlyHash<IContiguousRegion<TNode>> ContiguousRegions { get; }
}

public static class IRegionExt
{
        
}
