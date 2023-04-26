using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RoadChunkGraphicModule : MapChunkGraphicModule
{
    public RoadChunkGraphicModule(MapChunk chunk, Data data, MapGraphics mg)
    {
        var r = new RoadChunkGraphic(chunk, data, mg);
        AddLayer(new Vector2(0, 100), r);
    }
}
