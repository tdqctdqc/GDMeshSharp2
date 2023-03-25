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
        var mb = new MeshBuilder();
        foreach (var p in chunk.Polys)
        {
            foreach (var n in p.Neighbors.Entities())
            {
                if (p.Id > n.Id)
                {
                    var border = p.GetEdge(n, data);
                    if (data.Society.Roads.ByEdgeId.ContainsKey(border.Id))
                    {
                        var seg = data.Society.Roads.ByEdgeId[border.Id];
                        seg.Road.Model().Draw(mb, chunk.RelTo.GetOffsetTo(p.Center, data), 
                            chunk.RelTo.GetOffsetTo(n.Center, data), 10f);
                    }
                }
            }
        }
        if (mb.Tris.Count == 0) return;
        AddChild(mb.GetMeshInstance());
    }
}