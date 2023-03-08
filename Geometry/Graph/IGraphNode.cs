using Godot;
using System;
using System.Collections.Generic;

public interface IGraphNode<TNode, TEdge>
{
    IReadOnlyCollection<TNode> Neighbors { get; }
    TEdge GetEdge(TNode neighbor);
    bool HasEdge(TNode neighbor);
}
