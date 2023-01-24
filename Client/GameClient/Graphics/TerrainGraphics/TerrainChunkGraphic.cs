using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TerrainChunkGraphic : Node2D
{
    public void Setup<T>(List<MapPolygon> polys, Data data, TerrainAspectManager<T> manager) where T : TerrainAspect
    {
        var first = polys.First();
        var tris = new List<Vector2>();
        var colors = new List<Color>();
        for (var i = 0; i < polys.Count; i++)
        {
            var p = polys[i];
            var offset = first.GetOffsetTo(p, data);
            for (var j = manager.ByPriority.Count - 1; j >= 0; j--)
            {
                var aspect = manager.ByPriority[j];
                var aspectTris = data.Planet.TerrainTris.ByName[aspect.Name].GetPolyTris(p);
                if (aspectTris == null) continue;
                aspectTris.ForEach(t =>
                {
                    tris.Add(t.A + offset);
                    tris.Add(t.B + offset);
                    tris.Add(t.C + offset);
                    colors.Add(aspect.Color);
                });
            }
        }

        if (tris.Count == 0) return;
        var mesh = MeshGenerator.GetMeshInstance(tris.ToList(), colors);
        AddChild(mesh);
    }
}