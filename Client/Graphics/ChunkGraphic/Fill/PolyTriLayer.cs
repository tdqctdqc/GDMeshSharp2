using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Godot;

public class PolyTriLayer : MapChunkGraphicLayer
{
    private Func<PolyTri, Color> _getColor;

    public PolyTriLayer(MapChunk chunk, Data data, Func<PolyTri, Color> getColor) 
        : base(chunk, null)
    {
        _getColor = getColor;
        Draw(data);
    }

    public override void Draw(Data data)
    {
        var first = Chunk.RelTo;
        var mb = new MeshBuilder();
        foreach (var p in Chunk.Polys)
        {
            var offset = first.GetOffsetTo(p, data);
            var tris = p.Tris.Tris;
            for (var j = 0; j < tris.Length; j++)
            {
                var t = tris[j];
                // if (t.GetMinAltitude() < 10f) continue;
                mb.AddTri(t.Transpose(offset), 
                    _getColor(t)
                );
            }
        }

        if (mb.Tris.Count == 0) return;
        var mesh = mb.GetMeshInstance();
        AddChild(mesh);
    }
}
