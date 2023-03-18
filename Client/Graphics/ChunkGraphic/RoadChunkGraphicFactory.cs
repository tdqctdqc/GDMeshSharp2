
using System;
using Godot;

public class RoadChunkGraphicFactory : ChunkGraphicFactory
{
    public RoadChunkGraphicFactory(string name, bool active) 
        : base(name, active, (c,d) => new RoadChunkGraphic(c,d))
    {
    }
}
