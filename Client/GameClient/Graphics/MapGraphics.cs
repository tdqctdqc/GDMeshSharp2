using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class MapGraphics : Node2D
{
    public MapGraphics()
    {
        
    }
    protected List<IGraphicsSegmenter> _segmenters;
    public PolyHighlighter Highlighter { get; private set; }
    public List<MapChunkGraphic> MapChunkGraphics { get; private set; }
    public ChunkChangedCache ChunkChangedCache { get; private set; }
    private Data _data;
    public void Setup(Data data)
    {
        var sw = new Stopwatch();
        sw.Start();
        _data = data;
        ChunkChangedCache = new ChunkChangedCache(_data);
        Clear();
        _segmenters = new List<IGraphicsSegmenter>();
        MapChunkGraphics = new List<MapChunkGraphic>();
        var polySegmenter = new GraphicsSegmenter<MapChunkGraphic>();
        _segmenters.Add(polySegmenter);
        
        var mapChunkGraphics = _data.Planet.PolygonAux.Chunks.Select(u =>
        {
            var graphic = new MapChunkGraphic();
            MapChunkGraphics.Add(graphic);
            graphic.Setup(this, u, _data);
            return graphic;
        }).ToList();
        
        
        foreach (var keyValuePair in MapChunkLayerBenchmark.Times)
        {
           GD.Print($"{keyValuePair.Key} {keyValuePair.Value.Sum()}"); 
        }
        
        polySegmenter.Setup(mapChunkGraphics, 10, n => n.Position, _data);

        Highlighter = new PolyHighlighter(_data);
        Highlighter.ZIndex = 99;
        AddChild(Highlighter);
        
        AddChild(polySegmenter);
        var inputCatcher = new MapInputCatcher(_data, this);
        AddChild(inputCatcher);
        
        sw.Stop();
        GD.Print("map graphics setup time " + sw.Elapsed.TotalMilliseconds);
    }

    public void Update()
    {
        MapChunkGraphics.ForEach(c =>
        {
            c.Update();
        });
        ChunkChangedCache.Clear();
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