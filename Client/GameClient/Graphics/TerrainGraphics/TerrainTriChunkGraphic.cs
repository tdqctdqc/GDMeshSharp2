using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TerrainTriChunkGraphic : Node2D
{
    public void Setup(MapChunk chunk, Data data, 
        Func<PolyTri, Color> getColor) 
    {
        var first = chunk.RelTo;
        var mb = new MeshBuilder();
        foreach (var p in chunk.Polys)
        {
            var offset = first.GetOffsetTo(p, data);
            var tris = p.GetTerrainTris(data).Tris;
            for (var j = 0; j < tris.Length; j++)
            {
                var t = tris[j];
                // if (t.GetMinAltitude() < 10f) continue;
                mb.AddTri(t.Transpose(offset), 
                    getColor(t)
                );
            }
        }

        if (mb.Tris.Count == 0) return;
        var mesh = mb.GetMeshInstance();
        AddChild(mesh);
    }
}