using Godot;
using System;
using System.Collections.Generic;


public interface IGraphNode<TNode>
{
    IReadOnlyCollection<TNode> Neighbors { get; }
    bool HasEdge(TNode neighbor);
}
public interface IGraphNode<TNode, TEdge> : IGraphNode<TNode>
{
    TEdge GetEdge(TNode neighbor);
}
