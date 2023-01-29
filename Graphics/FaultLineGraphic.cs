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
    public FaultLineGraphic(FaultLine f, GenData data)
    {
        FaultLine = f;
        if (f.PolyFootprint.Count == 0) return;

        var mb = new MeshBuilder();
        _segments = new Node2D();
        var footprintCol = new Color(Colors.Gray, .5f);
        mb.AddPolysRelative(FaultLine.Origin, f.PolyFootprint, p => footprintCol,
            data);
        
        f.Segments.ForEach(segs =>
        {
            mb.AddLines(segs, 20f, Colors.Red);
        });
        AddChild(mb.GetMeshInstance());
    }
}