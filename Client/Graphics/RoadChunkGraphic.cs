using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RoadChunkGraphic : Node2D
{
    public void Setup(MapChunk chunk, Data data)
    {
        var froms = new List<Vector2>();
        var tos = new List<Vector2>();
        foreach (var p in chunk.Polys)
        {
            foreach (var n in p.Neighbors.Refs())
            {
                if (p.Id > n.Id)
                {
                    var border = p.GetBorder(n, data);
                    if (data.Society.Roads.ByBorderId.ContainsKey(border.Id))
                    {
                        froms.Add(chunk.RelTo.GetOffsetTo(p.Center, data));
                        tos.Add(chunk.RelTo.GetOffsetTo(n.Center, data));
                    }
                }
            }
        }
        if (froms.Count == 0) return;
        GD.Print("adding roads");
        var mesh = MeshGenerator.GetLinesMesh(froms, tos, 10f);
        mesh.Modulate = Colors.LightGray;
        AddChild(mesh);
    }
}