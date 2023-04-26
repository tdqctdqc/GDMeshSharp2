using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class IconsMapChunkGraphicModule : MapChunkGraphicModule
{
    public IconsMapChunkGraphicModule(MapChunk chunk, Data data, MapGraphics mg)
    {
        var triIcons = new TriIconChunkGraphic(chunk, data, mg);
        AddLayer(new Vector2(0, 10), triIcons);
    }

    private IconsMapChunkGraphicModule()
    {
    }
}
