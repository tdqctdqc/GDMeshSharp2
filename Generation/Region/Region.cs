    using System.Collections.Generic;

    public class Region<TNode> : IRegion<TNode>
        where TNode : IGraphNode<TNode>
    {
        public IReadOnlyGraph<TNode> Graph { get; }
        public IReadOnlyHash<TNode> Elements => _elements.ReadOnly();
        private HashSet<TNode> _elements;
        public IReadOnlyHash<IContiguousRegion<TNode>> ContiguousRegions => _contiguous.ReadOnly();
        private HashSet<IContiguousRegion<TNode>> _contiguous;

        public Region(HashSet<TNode> elements, IReadOnlyGraph<TNode> graph)
        {
            Graph = graph;
            _elements = elements;
            _contiguous = ContiguousRegion<TNode>.GetRegions(elements).ToHashSet<IContiguousRegion<TNode>>();
            foreach (var c in _contiguous)
            {
                c.Split += rs => HandleSplit(c, rs);
            }
        }

        private void HandleSplit(IContiguousRegion<TNode> splitter, IEnumerable<IContiguousRegion<TNode>> newRegions)
        {
            _contiguous.Remove(splitter);
            _contiguous.AddRange(newRegions);
        }
    }
