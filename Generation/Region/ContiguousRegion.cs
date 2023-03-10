
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ContiguousRegion<TNode> : Region<TNode>, IContiguousRegion<TNode>
        where TNode : IGraphNode<TNode>
    {
        public Action<IEnumerable<IContiguousRegion<TNode>>> Split { get; set; }
        public IRegionBoundary<TNode> Border => _border;
        private RegionBoundary<TNode> _border;
        public IReadOnlyHash<IContiguousRegion<TNode>> K1Regions => _k1s.ReadOnly();
        private HashSet<IContiguousRegion<TNode>> _k1s;

        private Graph<IContiguousRegion<TNode>, Snake<TNode>> _bridges;

        public static IEnumerable<ContiguousRegion<TNode>> GetRegions(IEnumerable<TNode> elements)
        {
            var hash = new HashSet<TNode>(elements);
            var unions = UnionFind.Find<TNode>(elements, (a,b) => hash.Contains(a) == hash.Contains(b));
            throw new NotImplementedException();
        }

        private ContiguousRegion(HashSet<TNode> elements, IReadOnlyGraph<TNode> graph) : base(elements, graph)
        {
            _border = new RegionBoundary<TNode>(elements.ToList(), this);
            _border.CrossedSelf += HandleBoundarySelfCross;
        }

        public void AddNode(TNode n)
        {
            //check if node adjacent to any bridges
            throw new NotImplementedException();
        }

        private void HandleBoundarySelfCross(TNode cross)
        {
            throw new NotImplementedException();
        }
    }
