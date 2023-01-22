using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RoadChunkGraphic : Node2D
{
    public void Setup(List<MapPolygon> chunk, Data data)
    {
        var first = chunk.First();
        var froms = new List<Vector2>();
        var tos = new List<Vector2>();
        chunk.ForEach(p =>
        {
            foreach (var n in p.Neighbors.Refs())
            {
                if (p.Id > n.Id)
                {
                    var border = p.GetBorder(n, data);
                    if (data.Society.Roads.ByBorderId.ContainsKey(border.Id))
                    {
                        froms.Add(first.GetOffsetTo(p.Center, data.Planet.Width));
                        tos.Add(first.GetOffsetTo(n.Center, data.Planet.Width));
                    }
                }
            }
        });
        if (froms.Count == 0) return;
        var mesh = MeshGenerator.GetLinesMesh(froms, tos, 10f);
        mesh.Modulate = Colors.LightGray;
        AddChild(mesh);
    }
}