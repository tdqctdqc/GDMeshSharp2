using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SegmentsNotConnectedDisplay : Node2D
{
    public void Setup(SegmentsException e)
    {
        var mb = new MeshBuilder();
        var step = 3f;
        int iter = 0;
        e.SegLayers.ForEach(p =>
        {
            mb.AddArrows(p, (e.SegLayers.Count - iter + 1) * step, ColorsExt.GetRainbowColor(iter));
            mb.AddNumMarkers(p.Select(s => s.Mid()).ToList(), 20f, Colors.Transparent, Colors.White, Vector2.Zero);
            iter++;
        }); 
        
        e.PointSets.ForEach(p =>
        {
            mb.AddPointMarkers(p, step * 10f, ColorsExt.GetRainbowColor(iter));
            iter++;
        });
        AddChild(mb.GetMeshInstance());
    }

    
}