using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RoadChunkGraphic : Node2D
{
    private RoadChunkGraphic()
    {
        
    }

    public RoadChunkGraphic(MapChunk chunk, Data data)
    {
        var froms = new List<Vector2>();
        var tos = new List<Vector2>();
        foreach (var p in chunk.Polys)
        {
            foreach (var n in p.Neighbors.Refs())
            {
                if (p.Id > n.Id)
                {
                    var border = p.GetEdge(n, data);
                    if (data.Society.Roads.ByEdgeId.ContainsKey(border.Id))
                    {
                        froms.Add(chunk.RelTo.GetOffsetTo(p.Center, data));
                        tos.Add(chunk.RelTo.GetOffsetTo(n.Center, data));
                    }
                }
            }
        }
        if (froms.Count == 0) return;
        var outsideMesh = MeshGenerator.GetLinesMesh(froms, tos, 10f);
        outsideMesh.Modulate = Colors.LightGray;
        var insideMesh = MeshGenerator.GetLinesMesh(froms, tos, 8f);
        insideMesh.Modulate = Colors.Black.Lightened(.1f);
        AddChild(outsideMesh);
        AddChild(insideMesh);
    }
}