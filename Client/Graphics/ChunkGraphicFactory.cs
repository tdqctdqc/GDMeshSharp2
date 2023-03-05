
using System;
using Godot;

public class ChunkGraphicFactory
{
    public bool Active { get; private set; }
    public string Name { get; private set; }
    public Func<MapChunk, Data, Node2D> GetNode { get; private set; }

    public ChunkGraphicFactory(string name, bool active, Func<MapChunk, Data, Node2D> getNode)
    {
        Name = name;
        Active = active;
        GetNode = getNode;
    }
}
