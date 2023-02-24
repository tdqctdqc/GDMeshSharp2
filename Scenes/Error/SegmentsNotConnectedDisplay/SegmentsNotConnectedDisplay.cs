using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SegmentsNotConnectedDisplay : Control
{
    public void Setup(SegmentsNotConnectedException e)
    {
        
        var label = (Label)FindNode("Label");
        if(e.Poly != null) label.Text = "Poly " + e.Poly.Id.ToString();
        var mb = new MeshBuilder();
        var step = 10f;
        var partialWidth = e.Partials.Count * step;
        mb.AddArrows(e.SegsBefore, step * 2f + partialWidth, Colors.Black);
        int iter = 0;
        e.Partials.ForEach(p =>
        {
            mb.AddArrows(p, (e.Partials.Count - iter + 1) * step, ColorsExt.GetRainbowColor(iter));
            iter++;
        }); 
        
        mb.AddArrows(e.SegsAfter, step, Colors.White);
        mb.AddNumMarkers(e.SegsAfter.Select(s => s.Mid()).ToList(), 20f, Colors.Gray);
        
        
        // mb.AddNumMarkers(e.SegsBefore.Select(s => s.From).ToList(), 5f, Colors.Pink);
        // mb.AddNumMarkers(e.SegsBefore.Select(s => s.To).ToList(), 5f, Colors.Green);
        //
        //
        
        AddChild(mb.GetMeshInstance());
    }
}