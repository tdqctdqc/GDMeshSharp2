using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ChunkDecalGraphic : Node2D
{
    public void Setup(MapChunk chunk, MapPolygon relTo, Data data)
    {
        var tris = chunk.Polys
            .SelectMany(p => p.GetTerrainTris(data).Tris.Select(t => t.Transpose(relTo.GetOffsetTo(p, data))));
        var lfSort = tris.Sort(t => t.Landform);
        var vegSort = tris.Sort(t => t.Vegetation);
        foreach (var kvp in lfSort)
        {
            if (kvp.Key is IDecaledTerrain d)
            {
                SetupDecals(d, kvp.Value, relTo, data);
            }
        }
        foreach (var kvp in vegSort)
        {
            if (kvp.Key is IDecaledTerrain d)
            {
                SetupDecals(d, kvp.Value, relTo, data);
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

        var allPs = pts.Select(pt => pt.GetPoissonPointsInside(t.DecalSpacing)).ToList();

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
