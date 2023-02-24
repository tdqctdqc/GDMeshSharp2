using System;
using System.Linq;
using Godot;

public class BadTriangulationDisplay : Control
{
    public void Setup(BadTriangulationError err)
    {
        var mb = new MeshBuilder();
        for (var i = 0; i < err.Tris.Count; i++)
        {
            mb.AddTri(err.Tris[i], err.Colors[i]);
        }
        for (var i = 0; i < err.Outlines.Count; i++)
        {
            var segs = err.Outlines[i];
            for (var j = 0; j < segs.Count; j++)
            {
                mb.AddArrow(segs[j].From, segs[j].To, 1f, ColorsExt.GetRainbowColor(j));
            }
            mb.AddNumMarkers(segs.Select(s => s.Mid()).ToList(), 10f, Colors.Gray);

        }
        
        AddChild(mb.GetMeshInstance());
    }
}