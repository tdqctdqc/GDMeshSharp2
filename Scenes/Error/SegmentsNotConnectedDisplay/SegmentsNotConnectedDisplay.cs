using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SegmentsNotConnectedDisplay : Control
{
    public void Setup(SegmentsNotConnectedException e)
    {
        var mb = new MeshBuilder();
        var step = 3f;
        var partialWidth = e.Partials.Count * step;
        mb.AddArrows(e.SegsBefore, step * 2f + partialWidth, Colors.Black);
        int iter = 0;
        e.Partials.ForEach(p =>
        {
            mb.AddArrows(p, (e.Partials.Count - iter + 1) * step, ColorsExt.GetRainbowColor(iter));
            iter++;
        }); 
        
        mb.AddArrows(e.SegsAfter, step, Colors.White);
        mb.AddNumMarkers(e.SegsAfter.Select(s => s.Mid()).ToList(), 20f, Colors.Transparent, Colors.White, Vector2.Zero);
        mb.AddNumMarkers(e.SegsBefore.Select(s => s.Mid()).ToList(), 20f, Colors.Transparent, Colors.Black, Vector2.One * -10f);
        
        AddChild(mb.GetMeshInstance());
    }

    
}