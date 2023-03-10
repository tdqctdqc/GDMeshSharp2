    using System.Collections.Generic;
    using System.Linq;

    public class RegionBoundary<TNode> : Boundary<TNode>, IRegionBoundary<TNode>
        where TNode : IGraphNode<TNode>
    {
        public IRegion<TNode> Region { get; }
        public RegionBoundary(IEnumerable<TNode> elements, IRegion<TNode> region) 
            : base(region.Graph.GetOrderedBoundarySegs(elements))
        {
            Region = region;
        }
    }