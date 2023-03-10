using Godot;
using System;
using System.Collections.Generic;



public class GraphNode<TNode> : IGraphNode<TNode>
{
    public TNode Element {get; private set;}
    public HashSet<TNode> Neighbors { get; private set; }

    IReadOnlyCollection<TNode> IGraphNode<TNode>.Neighbors => Neighbors;

    public GraphNode(TNode element)
    {
        Element = element;
        Neighbors = new HashSet<TNode>();
    }

    public bool HasNeighbor(TNode t)
    {
        return Neighbors.Contains(t);
    }
    bool IGraphNode<TNode>.HasEdge(TNode neighbor) => HasNeighbor(neighbor);

}
public class GraphNode<TNode, TEdge> : GraphNode<TNode>, IGraphNode<TNode, TEdge>
{
    private Dictionary<TNode, GraphNode<TNode, TEdge>> _nodeDic;
    private Dictionary<TNode, TEdge> _costs;
    public GraphNode(TNode element) : base(element)
    {
        _nodeDic = new Dictionary<TNode, GraphNode<TNode, TEdge>>();
        _costs = new Dictionary<TNode, TEdge>();
    }

    
    public TEdge GetEdgeCost(GraphNode<TNode, TEdge> neighbor)
    {
        return _costs[neighbor.Element];
    }
    public TEdge GetEdgeCost(TNode neighbor)
    {
        return _costs[neighbor];
    }
    public void SetEdgeValue(GraphNode<TNode, TEdge> neighbor, TEdge newEdgeVal)
    {
        _costs[neighbor.Element] = newEdgeVal;
    }
    public void AddNeighbor(GraphNode<TNode, TEdge> neighbor, TEdge edge)
    {
        if (_costs.ContainsKey(neighbor.Element))
        {
            return;
        }
        Neighbors.Add(neighbor.Element);
        _nodeDic.Add(neighbor.Element, neighbor);
        _costs.Add(neighbor.Element, edge);
    }

    public void RemoveNeighbor(GraphNode<TNode, TEdge> neighbor)
    {
        Neighbors.Remove(neighbor.Element);
        _costs.Remove(neighbor.Element);
        _nodeDic.Remove(neighbor.Element);
    }
    public void AddNeighbor(TNode poly, TEdge edge)
    {
        var node = new GraphNode<TNode, TEdge>(poly);
        AddNeighbor(node, edge);
    }

    public void RemoveNeighbor(TNode neighbor)
    {
        RemoveNeighbor(_nodeDic[neighbor]);
    }
    
    TEdge IGraphNode<TNode, TEdge>.GetEdge(TNode neighbor) => _costs[neighbor];
}
