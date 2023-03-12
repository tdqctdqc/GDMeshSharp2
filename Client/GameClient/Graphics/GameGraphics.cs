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
    private TooltipManager _tooltips;
    private MapPolyTooltip _polyTooltip;
    private IClient _client;
    public PolyHighlighter Highlighter { get; private set; }
    public void Process(float delta, Data data)
    {
        if(_client?.Cam != null)
        {
            _segmenters?.ForEach(s => s.Update(_client.Cam.XScrollRatio));
            _tooltips?.Process(delta, data, _client.Cam.GetMousePosInMapSpace(data));
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
    public void Setup(IClient client, Data data)
    {
        Clear();

        _client = client;
        
        _segmenters = new List<IGraphicsSegmenter>();
        MapChunkGraphics = new List<MapChunkGraphic>();
        var polySegmenter = new GraphicsSegmenter<MapChunkGraphic>();
        _segmenters.Add(polySegmenter);
        int iter = 0;
        var mapChunks = data.Cache.Chunks.Select(u =>
        {
            var graphic = new MapChunkGraphic();

            MapChunkGraphics.Add(graphic);

            graphic.Setup(u, data);

            return graphic;
        }).ToList();


        Highlighter = new PolyHighlighter();
        Highlighter.ZIndex = 99;
        Highlighter.ZAsRelative = false;
        AddChild(Highlighter);
        
        polySegmenter.Setup(mapChunks, 10, n => n.Position, data);
        AddChild(polySegmenter);
        _polyTooltip = SceneManager.Instance<MapPolyTooltip>();
        _polyTooltip.ZIndex = 99;
        _polyTooltip.ZAsRelative = false;
        AddChild(_polyTooltip);
        _tooltips = new TooltipManager(_polyTooltip, Highlighter, _client);
    }
}