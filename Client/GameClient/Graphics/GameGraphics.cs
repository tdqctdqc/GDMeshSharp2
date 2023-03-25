using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class GameGraphics : Node2D
{
    public static GameGraphics Get() => (GameGraphics) ((PackedScene)GD.Load("res://Client/GameClient/Graphics/GameGraphics.tscn")).Instance();
    protected List<IGraphicsSegmenter> _segmenters;
    public PolyHighlighter Highlighter { get; private set; }
    public List<MapChunkGraphic> MapChunkGraphics { get; private set; }
    private Data _data;
    private MouseOverPolyHandler _mousePolyHandler;
    public void Setup(Data data)
    {
        _data = data;
        _mousePolyHandler = new MouseOverPolyHandler();
        Clear();
        _segmenters = new List<IGraphicsSegmenter>();
        MapChunkGraphics = new List<MapChunkGraphic>();
        var polySegmenter = new GraphicsSegmenter<MapChunkGraphic>();
        _segmenters.Add(polySegmenter);
        var mapChunkGraphics = data.Planet.Polygons.Chunks.Select(u =>
        {
            var graphic = new MapChunkGraphic();

            MapChunkGraphics.Add(graphic);

            graphic.Setup(u, data);

            return graphic;
        }).ToList();
        polySegmenter.Setup(mapChunkGraphics, 10, n => n.Position, data);

        Highlighter = new PolyHighlighter();
        Highlighter.ZIndex = 99;
        Highlighter.ZAsRelative = false;
        AddChild(Highlighter);
        
        AddChild(polySegmenter);
    }
    public void Process(float delta)
    {
        if(Game.I.Client?.Cam != null)
        {
            _segmenters?.ForEach(s => s.Update(Game.I.Client.Cam.XScrollRatio));
        }
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e is InputEventMouseMotion mm)
        {
            var mapPos = Game.I.Client.Cam.GetMousePosInMapSpace();
            var d = GetProcessDeltaTime();
            _mousePolyHandler?.Process(d, _data, mapPos);
        }
    }

    private void Clear()
    {
        _segmenters?.Clear();
        while (GetChildCount() > 0)
        {
            GetChild(0).Free();
        }
    }
}