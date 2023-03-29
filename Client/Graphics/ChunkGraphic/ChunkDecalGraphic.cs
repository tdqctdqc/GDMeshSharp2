using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ChunkDecalGraphic : Node2D
{
    public void Setup(MapChunk chunk, Data data)
    {
        var tris = chunk.Polys
            .SelectMany(p => p.Tris.Tris.Select(t => t.Transpose(chunk.RelTo.GetOffsetTo(p, data))));
        var lfSort = tris.Sort(t => t.Landform);
        var vegSort = tris.Sort(t => t.Vegetation);
        foreach (var kvp in lfSort)
        {
            if (kvp.Key is IDecaledTerrain d)
            {
                SetupDecals(d, kvp.Value, chunk.RelTo, data);
            }
        }
        foreach (var kvp in vegSort)
        {
            if (kvp.Key is IDecaledTerrain d)
            {
                SetupDecals(d, kvp.Value, chunk.RelTo, data);
            }
        }
    }

    private void SetupDecals<T>(T t, List<PolyTri> pts, MapPolygon relTo, Data data) where T : IDecaledTerrain
    {
        var mesh = t.GetDecal();
        var mi = new MultiMeshInstance2D();
        var mm = new MultiMesh();
        mm.Mesh = mesh;
        mm.ColorFormat = MultiMesh.ColorFormatEnum.Float;

        mi.Multimesh = mm;

        var allPs = pts.Select(pt =>
                {
                    var poisson = pt.GetPoissonPointsInside(t.DecalSpacing);
                    if (poisson.Count > 0) return poisson;
                    else return new List<Vector2>{pt.GetCentroid()};
                })
            .ToList();

        mm.InstanceCount = allPs.Sum(ps => ps.Count);
        int iter = 0;
        for (var i = 0; i < pts.Count; i++)
        {
            var ps = allPs[i];
            var pt = pts[i];
            for (var j = 0; j < ps.Count; j++)
            {
                mm.SetInstanceTransform2d(iter, new Transform2D(0f, ps[j]));
                mm.SetInstanceColor(iter, t.GetDecalColor(pt));
                iter++;
            }
            
        }
        AddChild(mi);

        
        
        
        
        
        // var cols = new List<Color>();
        
        //
        // for (var i = 0; i < allPs.Count; i++)
        // {
        //     mm.SetInstanceTransform2d(i, new Transform2D(0f, allPs[i] + pos));
        // }

    }
}
