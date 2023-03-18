
using System;
using Godot;

public class RoadChunkGraphicFactory : ChunkGraphicFactory
{
    public RoadChunkGraphicFactory(string name, bool active) 
        : base(name, active)
    {
    }

    public override Node2D GetNode(MapChunk c, Data d)
    {
        return new RoadChunkGraphic(c, d);
    }
}
