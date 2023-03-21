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
    public IClient Client { get; private set; }
    public PolyHighlighter Highlighter { get; private set; }
    public void Process(float delta, Data data)
    {
        if(Client?.Cam != null)
        {
            _segmenters?.ForEach(s => s.Update(Client.Cam.XScrollRatio));
            _tooltips?.Process(delta, data, Client.Cam.GetMousePosInMapSpace(data));
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

    public void SetClient(IClient client)
    {
        Client = client;
    }
    public void Setup(Data data)
    {
        Clear();
        _segmenters = new List<IGraphicsSegmenter>();
        MapChunkGraphics = new List<MapChunkGraphic>();
        var polySegmenter = new GraphicsSegmenter<MapChunkGraphic>();
        _segmenters.Add(polySegmenter);
        int iter = 0;
        var mapChunks = data.Planet.Polygons.Chunks.Select(u =>
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
        _tooltips = new TooltipManager(_polyTooltip, Highlighter, this);
    }
}