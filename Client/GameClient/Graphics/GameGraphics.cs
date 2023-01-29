using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class GameGraphics : Node2D
{
    public static GameGraphics Get() => (GameGraphics) ((PackedScene)GD.Load("res://Client/GameClient/Graphics/GameGraphics.tscn")).Instance();
    protected List<IGraphicsSegmenter> _segmenters;
    public List<MapChunkGraphic> MapChunkGraphics { get; private set; }
    private CameraController _cam;
    public override void _Process(float delta)
    {
        if(_cam != null)
        {
            _segmenters.ForEach(s => s.Update(_cam.XYRatio));
        }
    }
    private void Clear()
    {
        _segmenters.Clear();
        while (GetChildCount() > 0)
        {
            GetChild(0).Free();
        }
    }
    public void Setup(IClient client, Data data, CameraController cam)
    {
        _cam = cam;
        _segmenters = new List<IGraphicsSegmenter>();
        MapChunkGraphics = new List<MapChunkGraphic>();
        Clear();
        
        var polySegmenter = new GraphicsSegmenter<MapChunkGraphic>();
        _segmenters.Add(polySegmenter);
        var mapChunks = data.Cache.Chunks.Select(u =>
        {
            var graphic = new MapChunkGraphic();
            MapChunkGraphics.Add(graphic);
            graphic.Setup(u, data);
            return graphic;
        }).ToList();
        polySegmenter.Setup(mapChunks, 10, n => n.Position, data);
        AddChild(polySegmenter);
    }
}