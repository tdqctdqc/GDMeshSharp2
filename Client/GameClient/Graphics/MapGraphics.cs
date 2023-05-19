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
    public List<MapChunkGraphicHolder> MapChunkGraphics { get; private set; }
    public ChunkChangedCache ChunkChangedCache { get; private set; }
    private TimerAction _updateChunks;
    private Data _data;
    public void Setup(Data data)
    {
        _data = data;
        ChunkChangedCache = new ChunkChangedCache(_data);
        Clear();
        _segmenters = new List<IGraphicsSegmenter>();
        MapChunkGraphics = new List<MapChunkGraphicHolder>();
        var polySegmenter = new GraphicsSegmenter<MapChunkGraphicHolder>();
        _segmenters.Add(polySegmenter);
        var mapChunkGraphics = _data.Planet.PolygonAux.Chunks.Select(u =>
        {
            var graphic = new MapChunkGraphicHolder();
            MapChunkGraphics.Add(graphic);
            graphic.Setup(this, u, _data);
            return graphic;
        }).ToList();
        
        polySegmenter.Setup(mapChunkGraphics, 10, n => n.Position, _data);

        Highlighter = new PolyHighlighter(_data);
        Highlighter.ZIndex = 99;
        Highlighter.ZAsRelative = false;
        AddChild(Highlighter);
        
        AddChild(polySegmenter);
        var inputCatcher = new MapInputCatcher(_data, this);
        AddChild(inputCatcher);
        _updateChunks = new TimerAction(.5f, 0f, () =>
            {
                MapChunkGraphics.ForEach(c => c.Update());
                ChunkChangedCache.Clear();
            }
        );

        
    }

    public void Process(float delta)
    {
        if(Game.I.Client?.Cam != null)
        {
            _segmenters?.ForEach(s => s.Update(Game.I.Client.Cam.XScrollRatio));
        }

        _updateChunks?.Process(delta);
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