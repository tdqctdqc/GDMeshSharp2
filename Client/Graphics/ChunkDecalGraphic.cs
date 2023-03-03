using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ChunkDecalGraphic : Node2D
{
    private MultiMeshInstance2D _lfMi, _vegMi;
    private Func<PolyTri, List<Vector2>> _getDecalPoses = pt => new List<Vector2> {pt.GetCentroid()};
    private Dictionary<Landform, Mesh> _lfDecals;
    private Dictionary<Landform, MultiMeshInstance2D> _lfMis;
    private Dictionary<Vegetation, Mesh> _vegDecals;
    private Dictionary<Vegetation, MultiMeshInstance2D> _vegMis;

    public void Setup(MapChunk chunk, MapPolygon relTo, Data data)
    {
        var mb = new MeshBuilder();
        int iter = 0;
        foreach (var poly in chunk.Polys)
        {
            var offset = relTo.GetOffsetTo(poly, data);
            var pts = poly.GetTerrainTris(data).Tris;
            for (var i = 0; i < pts.Length; i++)
            {
                var pt = poly.GetTerrainTris(data).Tris[i];
                if (pt.Landform is IDecaledTerrain d2)
                {
                    iter++;
                    d2.GetDecal(mb, pt, offset);
                }
                if (pt.Vegetation is IDecaledTerrain d1)
                {
                    iter++;
                    d1.GetDecal(mb, pt, offset);
                }
                
            }
        }
        if(iter > 0) AddChild(mb.GetMeshInstance());
    }
}
