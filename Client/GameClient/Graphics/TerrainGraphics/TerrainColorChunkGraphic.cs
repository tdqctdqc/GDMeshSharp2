using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TerrainColorChunkGraphic : Node2D
{
    public void Setup<T>(List<MapPolygon> polys, Data data, TerrainAspectManager<T> manager) where T : TerrainAspect
    {
        var first = polys.First();
        var mb = new MeshBuilder();
        for (var i = 0; i < polys.Count; i++)
        {
            var p = polys[i];
            var offset = first.GetOffsetTo(p, data);
            for (var j = manager.ByPriority.Count - 1; j >= 0; j--)
            {
                var aspect = manager.ByPriority[j];
                var aspectTris = p.TerrainTris[aspect];
                if (aspectTris == null) continue;
                aspectTris.ForEach(t =>
                {
                    mb.AddTri(t.Transpose(offset), aspect.Color);
                });
            }
        }

        if (mb.Tris.Count == 0) return;
        var mesh = mb.GetMeshInstance();
        AddChild(mesh);
    }
}