using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Godot;

public class RoadChunkGraphic : MapChunkGraphicLayer
{
    private RoadChunkGraphic()
    {
        
    }

    public RoadChunkGraphic(MapChunk chunk, Data data, MapGraphics mg) 
        : base(chunk, mg.ChunkChangedCache.RoadsChanged)
    {
        Draw(data);
    }


    protected override void Draw(Data data)
    {
        this.ClearChildren();
        var froms = new List<Vector2>();
        var tos = new List<Vector2>();
        var mb = new MeshBuilder();
        foreach (var p in Chunk.Polys)
        {
            foreach (var n in p.Neighbors.Entities())
            {
                if (p.Id > n.Id)
                {
                    var border = p.GetEdge(n, data);
                    if (data.Society.RoadAux.ByEdgeId.ContainsKey(border.Id))
                    {
                        var seg = data.Society.RoadAux.ByEdgeId[border.Id];
                        seg.Road.Model().Draw(mb, Chunk.RelTo.GetOffsetTo(p.Center, data), 
                            Chunk.RelTo.GetOffsetTo(n.Center, data), 10f);
                    }
                }
            }
        }
        if (mb.Tris.Count == 0) return;
        AddChild(mb.GetMeshInstance());
    }
}