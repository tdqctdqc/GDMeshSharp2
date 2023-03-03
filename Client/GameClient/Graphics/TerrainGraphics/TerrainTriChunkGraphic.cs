using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TerrainTriChunkGraphic : Node2D
{
    public void Setup(List<MapPolygon> polys, Data data, 
        Func<PolyTri, Color> getColor) 
    {
        var first = polys.First();
        var mb = new MeshBuilder();
        for (var i = 0; i < polys.Count; i++)
        {
            var p = polys[i];
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