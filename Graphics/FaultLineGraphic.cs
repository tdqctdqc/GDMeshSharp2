using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FaultLineGraphic : Node2D
{
    public FaultLine FaultLine { get; private set; }
    private Node2D _segments, _footprint;
    public Vector2 Origin => FaultLine.Origin.Center;

    public FaultLineGraphic()
    {
        
    }
    public FaultLineGraphic(FaultLine f, WorldData data)
    {
        FaultLine = f;
        if (f.PolyFootprint.Count == 0) return;
        _segments = new Node2D();
        
        
        var tris = new List<Vector2>();
        f.PolyFootprint.ForEach(p =>
        {
            var offset = p.GetOffsetTo(Origin, data.Dimensions.x);
            var trisRel = p.GetTrisRel().Select(v => v - offset);
            tris.AddRange(trisRel);
        });
        _footprint = MeshGenerator.GetMeshInstance(tris);
        _footprint.Modulate = new Color(Colors.Gray, .5f);
        AddChild(_footprint);
        
        
        f.Segments.ForEach(segs =>
        {
            var segLines = MeshGenerator.GetLinesMesh(segs, Vector2.Zero, 20f, false);
            var segMarkers = MeshGenerator.GetPointsMesh(segs.Select(s => s.From).ToList(), 50f);
            _segments.AddChild(segLines);
            _segments.AddChild(segMarkers);
        });
        _segments.Modulate = Colors.Red;
        AddChild(_segments);
    }
}