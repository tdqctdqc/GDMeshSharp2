
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BorderChunkGraphic : MapChunkGraphicModule
{
    public static BorderChunkGraphic ConstructRegimeBorder(MapChunk chunk, MapGraphics mg, float thickness, Data data)
    {
        return new BorderChunkGraphic(chunk, mg, thickness, data);
    }

    private BorderChunkGraphic(MapChunk chunk, MapGraphics mg, float thickness, Data data)
    {
        var layer = new RegimeBorderChunkLayer(chunk, thickness, data, mg);
        AddLayer(new Vector2(0, 100), layer);
    }
    private BorderChunkGraphic()
    {
        
    }
    
}
