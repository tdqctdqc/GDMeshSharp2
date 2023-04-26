using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapChunkGraphicModule : Node2D
{
    private Dictionary<MapChunkGraphicLayer, Vector2> _layers;
    public MapChunkGraphicModule()
    {
        _layers = new Dictionary<MapChunkGraphicLayer, Vector2>();
    }

    protected void AddLayer(Vector2 range, MapChunkGraphicLayer layer)
    {
        AddChild(layer);
        _layers.Add(layer, range);
    }
    public void Update(Data data)
    {
        var zoom = Game.I.Client.Cam.ZoomOut;
        foreach (var kvp in _layers)
        {
            var range = kvp.Value;
            if (zoom >= range.x && zoom <= range.y)
            {
                kvp.Key.Visible = true;
                kvp.Key.Update(data);
            }
            else
            {
                kvp.Key.Visible = false;
            }
        }
    }
}
