using Godot;
using System;
using System.Collections.Generic;

public interface IGraphNode<TNode, TEdge>
{
    IReadOnlyList<TNode> Neighbors { get; }
    TEdge GetPolyBorder(TNode neighbor);
    void AddNeighbor(TNode poly, TEdge edge);
    void RemoveNeighbor(TNode neighbor);
}
