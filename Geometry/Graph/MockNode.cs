
    using System;
    using System.Collections.Generic;

    public class MockNode<TNode> 
        : IStaticNode<TNode>, IStaticNode<MockNode<TNode>>
    {
        public TNode Element { get; private set; }
        public IReadOnlyGraph<TNode> Graph { get; private set; }
        public MockNode(TNode element, IReadOnlyGraph<TNode> graph)
        {
            Element = element;
        }

        MockNode<TNode> IStaticNode<MockNode<TNode>>.Element => this;
        
        IReadOnlyGraph<MockNode<TNode>> IStaticNode<MockNode<TNode>>.Graph => _g;
        
        private static IReadOnlyGraph<MockNode<TNode>> _g 
            = new ImplicitGraph<MockNode<TNode>,bool>(
                n => true, 
                n => (IReadOnlyCollection<MockNode<TNode>>)n.Graph.GetNeighbors(n.Element), 
                (m,n) => n.Graph.HasEdge(n.Element, m.Element),
                (m,n) => n.Graph.HasEdge(n.Element, m.Element) ? true : throw new Exception()
            );
        public IReadOnlyCollection<TNode> Neighbors { get; }
        public bool HasEdge(TNode neighbor) => Graph.HasEdge(Element, neighbor);

        IReadOnlyCollection<MockNode<TNode>> IGraphNode<MockNode<TNode>>.Neighbors => (IReadOnlyCollection<MockNode<TNode>>)Neighbors;
        bool IGraphNode<MockNode<TNode>>.HasEdge(MockNode<TNode> neighbor) => Graph.HasEdge(Element, neighbor.Element);
    }
