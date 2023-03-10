
    using System;
    using System.Collections.Generic;

    public class MockStaticGraphNode<TNode> : IStaticGraphNode<TNode>, IStaticGraphNode<MockStaticGraphNode<TNode>>
    {
        public TNode Element { get; private set; }
        public IReadOnlyGraph<TNode> Graph { get; private set; }

        public MockStaticGraphNode(TNode element, IReadOnlyGraph<TNode> graph)
        {
            Element = element;
        }

        MockStaticGraphNode<TNode> IStaticGraphNode<MockStaticGraphNode<TNode>>.Element => this;
        
        IReadOnlyGraph<MockStaticGraphNode<TNode>> IStaticGraphNode<MockStaticGraphNode<TNode>>.Graph => _g;
        
        private static IReadOnlyGraph<MockStaticGraphNode<TNode>> _g 
            = new ImplicitGraph<MockStaticGraphNode<TNode>,bool>(
                n => true, 
                n => (IReadOnlyCollection<MockStaticGraphNode<TNode>>)n.Graph.GetNeighbors(n.Element), 
                (m,n) => n.Graph.HasEdge(n.Element, m.Element),
                (m,n) => n.Graph.HasEdge(n.Element, m.Element) ? true : throw new Exception()
            );
    }
