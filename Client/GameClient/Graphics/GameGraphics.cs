using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class GameGraphics : Node2D
{
    public static GameGraphics Get() => (GameGraphics) ((PackedScene)GD.Load("res://Client/GameClient/Graphics/GameGraphics.tscn")).Instance();
    protected List<IGraphicsSegmenter> _segmenters;
    public List<MapChunkGraphic> MapChunks { get; private set; }
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
    public void Setup(Data data, CameraController cam)
    {
        _cam = cam;
        _segmenters = new List<IGraphicsSegmenter>();
        // _client = client;
        var polyUnions = data.Planet.Polygons.GetPlateUnions();
        MapChunks = new List<MapChunkGraphic>();
        Clear();
        
        var polySegmenter = new GraphicsSegmenter<MapChunkGraphic>();
        _segmenters.Add(polySegmenter);
        var mapChunks = polyUnions.Select(u =>
        {
            var graphic = new MapChunkGraphic();
            MapChunks.Add(graphic);
            graphic.Setup(u, data);
            return graphic;
        }).ToList();
        polySegmenter.Setup(mapChunks, 10, n => n.Position, data);
        AddChild(polySegmenter);
    }
}