using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class GeneratorGraphics : Node2D
{
    private List<IGraphicsSegmenter> _segmenters; 
    private CameraController _cam;
    private GeneratorClient _client;
    public List<PolygonGraphic> PolyGraphics { get; private set; }

    public override void _Ready()
    {
        _segmenters = new List<IGraphicsSegmenter>();
    }

    private void Clear()
    {
        
        _segmenters.Clear();
        while (GetChildCount() > 0)
        {
            GetChild(0).Free();
        }
        _cam = new CameraController();
        AddChild(_cam);
        _cam.Current = true;
    }
    public void Setup(WorldData data, GeneratorClient client)
    {
        Clear();
        _client = client;
        var sw = new Stopwatch();
        sw.Start();
        
        PolyGraphics = data.PlanetDomain.GeoPolygons.Entities.Select(p => new PolygonGraphic(p)).ToList();
        
        var polySegmenter = new GraphicsSegmenter<PolygonGraphic>();
        _segmenters.Add(polySegmenter);
        polySegmenter.Setup(PolyGraphics, 10, p => p.Poly.Center, data);
        client.Holder.AddView(polySegmenter, "polygons");
        AddChild(polySegmenter);
        
        var landformTris = Graphics.GetTerrainAspectTrisGraphics(data.Landforms, data);
        _segmenters.Add(landformTris);
        client.Holder.AddOverlay("polygons", "Landform Tris", landformTris);
        AddChild(landformTris);
        
        var vegTris = Graphics.GetTerrainAspectTrisGraphics(data.Vegetation, data);
        _segmenters.Add(vegTris);
        client.Holder.AddOverlay("polygons", "Vegetation Tris", vegTris);
        AddChild(vegTris);
        
        var roadGraphics = Graphics.RoadGraphics(data);
        _segmenters.Add(roadGraphics);
        client.Holder.AddOverlay("polygons", "Roads", roadGraphics);
        AddChild(roadGraphics);

        var regimeGraphicsSegments = new GraphicsSegmenter<Node2D>();
        var plateRegimeGraphics = data.GenAuxData.Plates.Select(p => RegimeTerritoryGraphic.Get(p, data)).ToList();
        regimeGraphicsSegments.Setup(plateRegimeGraphics, 10, n => n.Position, data);
        _segmenters.Add(regimeGraphicsSegments);
        client.Holder.AddOverlay("polygons", "Regimes", regimeGraphicsSegments);
        AddChild(regimeGraphicsSegments);
        
        var faultLineGraphics = data.GenAuxData.FaultLines.Select(f => new FaultLineGraphic(f, data)).ToList();
        
        var faultLineNode = new GraphicsSegmenter<FaultLineGraphic>();
        _segmenters.Add(faultLineNode);
        faultLineNode.Setup(faultLineGraphics, 10, m => m.Origin, data);
        _client.Holder.AddOverlay("polygons", "fault lines", faultLineNode);
        AddChild(faultLineNode);
        
        sw.Stop();
        GD.Print("Graphics gen time " + sw.ElapsedMilliseconds / 1000f);
        sw.Reset();
    }

    public override void _Process(float delta)
    {
        _segmenters.ForEach(s => s.Update(_cam.XYRatio));
    }
}