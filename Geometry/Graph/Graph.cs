using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Graph<TNode, TEdge>
{
    public GraphNode<TNode, TEdge> this[TNode t] => _nodeDic[t];
    private Dictionary<TNode, GraphNode<TNode, TEdge>> _nodeDic;
    public List<TNode> Elements { get; private set; }
    public List<GraphNode<TNode, TEdge>> Nodes { get; private set; }
    public HashSet<TEdge> Edges { get; private set; }
    public Dictionary<TNode, SubGraph<TNode, TEdge>> NodeSubGraphs 
        { get; private set; }
    public Graph()
    {
        _nodeDic = new Dictionary<TNode, GraphNode<TNode, TEdge>>();
        Elements = new List<TNode>();
        Nodes = new List<GraphNode<TNode, TEdge>>();
        // SubGraphs = new List<SubGraph<TNode, TEdge>>();
        NodeSubGraphs = new Dictionary<TNode, SubGraph<TNode, TEdge>>();
        Edges = new HashSet<TEdge>();
    }
    
    public bool HasEdge(TNode t1, TNode t2)
    {
        if (_nodeDic.ContainsKey(t1) == false) return false;
        return _nodeDic[t1].HasNeighbor(t2);
    }

    public void SetEdgeValue(TNode t1, TNode t2, TEdge newEdgeVal)
    {
        var node1 = _nodeDic[t1];
        var node2 = _nodeDic[t2];
        node1.SetEdgeValue(node2, newEdgeVal);
        node2.SetEdgeValue(node1, newEdgeVal);
    }
    public void AddEdge(TNode t1, TNode t2, TEdge edge)
    {
        if(_nodeDic.ContainsKey(t1) == false) AddNode(t1);
        if(_nodeDic.ContainsKey(t2) == false) AddNode(t2);
        AddUndirectedEdge(t1, t2, edge);
    }
    public void AddDirectedEdge(GraphNode<TNode, TEdge> from, 
        GraphNode<TNode, TEdge> to, TEdge edge)
    {
        Edges.Add(edge);
        from.AddNeighbor(to, edge);
    }
    private void AddUndirectedEdge(TNode from, TNode to, TEdge edge)
    {
        Edges.Add(edge);
        var fromNode = _nodeDic[from];
        var toNode = _nodeDic[to];
        fromNode.AddNeighbor(toNode, edge);
        toNode.AddNeighbor(fromNode, edge);
    }
    public void AddDirectedEdge(TNode from, TNode to, TEdge edge)
    {
        Edges.Add(edge);
        var fromNode = _nodeDic[from];
        var toNode = _nodeDic[to];
        fromNode.AddNeighbor(toNode, edge);
    }
    public TEdge GetEdge(TNode t1, TNode t2)
    {
        var node1 = _nodeDic[t1];
        var node2 = _nodeDic[t2];
        return node1.GetEdgeCost(node2);
    }
    public void AddNode(GraphNode<TNode, TEdge> node)
    {
        _nodeDic.Add(node.Element, node);
        Elements.Add(node.Element);
        Nodes.Add(node);
    }
    public void AddNode(TNode element)
    {
        Elements.Add(element);
        var node = new GraphNode<TNode, TEdge>(element);
        _nodeDic.Add(node.Element, node);
        Nodes.Add(node);
    }
    
    public void AddUndirectedEdge(GraphNode<TNode, TEdge> from, 
        GraphNode<TNode, TEdge> to, TEdge edge)
    {
        Edges.Add(edge);
        from.AddNeighbor(to, edge);
        to.AddNeighbor(from, edge);
    }
    public bool Contains(TNode value)
    {
        return _nodeDic.ContainsKey(value);
    }
    public bool Remove(TNode value)
    {
        GraphNode<TNode, TEdge> nodeToRemove = _nodeDic[value];
        if (nodeToRemove == null) return false;
        Elements.Remove(value);
        _nodeDic.Remove(nodeToRemove.Element);

        foreach (var neighbor in nodeToRemove.Neighbors)
        {
            var nNode = _nodeDic[neighbor];
            nNode.RemoveNeighbor(nodeToRemove);
        }
        return true;
    }

    public HashSet<TNode> GetNeighbors(TNode value)
    {
        return _nodeDic[value].Neighbors;
    }
}