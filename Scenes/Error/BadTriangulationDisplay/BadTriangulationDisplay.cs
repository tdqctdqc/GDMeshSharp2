using System;
using System.Linq;
using Godot;

public class BadTriangulationDisplay : Control
{
    public void Setup(BadTriangulationError err)
    {
        var label = (Label) FindNode("Label");
        
        
        var mb = new MeshBuilder();
        for (var i = 0; i < err.Tris.Count; i++)
        {
            var inscribe = err.Tris[i].GetInscribed(.9f);
            mb.AddTri(inscribe, err.Colors[i]);
        }
        for (var i = 0; i < err.Outlines.Count; i++)
        {
            var segs = err.Outlines[i];
            var col = ColorsExt.GetRainbowColor(i);
            for (var j = 0; j < segs.Count; j++)
            {
                mb.AddArrow(segs[j].From, 
                    segs[j].To, 1f, 
                    col
                );
            }
            mb.AddNumMarkers(segs.Select(s => s.Mid()).ToList(), 
                10f, Colors.Transparent, Colors.White, Vector2.Zero);

        }
        
        AddChild(mb.GetMeshInstance());
    }
}