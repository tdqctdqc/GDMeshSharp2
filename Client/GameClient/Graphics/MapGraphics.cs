using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class MapGraphics : Node2D
{

    public MapGraphics()
    {
        
    }
    protected List<IGraphicsSegmenter> _segmenters;
    public PolyHighlighter Highlighter { get; private set; }
    public List<MapChunkGraphic> MapChunkGraphics { get; private set; }
    private Data _data;
    public void Setup(Data data)
    {
        _data = data;
        Clear();
        _segmenters = new List<IGraphicsSegmenter>();
        MapChunkGraphics = new List<MapChunkGraphic>();
        var polySegmenter = new GraphicsSegmenter<MapChunkGraphic>();
        _segmenters.Add(polySegmenter);
        var mapChunkGraphics = data.Planet.PolygonAux.Chunks.Select(u =>
        {
            var graphic = new MapChunkGraphic();

            MapChunkGraphics.Add(graphic);

            graphic.Setup(u, data);

            return graphic;
        }).ToList();
        
        
        
        polySegmenter.Setup(mapChunkGraphics, 10, n => n.Position, data);

        Highlighter = new PolyHighlighter(_data);
        Highlighter.ZIndex = 99;
        Highlighter.ZAsRelative = false;
        AddChild(Highlighter);

        
        AddChild(polySegmenter);
        var inputCatcher = new MapInputCatcher(_data, this);
        AddChild(inputCatcher);
    }
    public void Process(float delta)
    {
        if(Game.I.Client?.Cam != null)
        {
            _segmenters?.ForEach(s => s.Update(Game.I.Client.Cam.XScrollRatio));
        }
    }

    public override void _Input(InputEvent e)
    {
        
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