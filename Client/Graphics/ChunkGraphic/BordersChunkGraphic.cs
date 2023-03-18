
using System.Collections.Generic;
using Godot;

public class BordersChunkGraphic : Node2D
{
    public void Setup(List<List<LineSegment>> segLists, List<float> thicknesses, List<Color> colors)
    {
        this.ClearChildren();
        var mb = new MeshBuilder();
        for (var i = 0; i < segLists.Count; i++)
        {
            var segs = segLists[i];
            var thickness = thicknesses[i];
            var color = colors[i];
            mb.AddLines(segs, thickness, color);
        }
        AddChild(mb.GetMeshInstance());
    }
    public void Setup(List<List<LineSegment>> segLists, List<Color> colors, float thickness)
    {
        this.ClearChildren();
        var mb = new MeshBuilder();
        for (var i = 0; i < segLists.Count; i++)
        {
            var segs = segLists[i];
            var color = colors[i];
            mb.AddLines(segs, thickness, color);
        }
        AddChild(mb.GetMeshInstance());
    }
}
