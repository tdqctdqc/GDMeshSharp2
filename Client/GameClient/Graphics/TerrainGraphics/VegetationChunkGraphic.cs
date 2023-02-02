using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class VegetationChunkGraphic : Node2D
{
    public void Setup(List<MapPolygon> polys, Data data)
    {
        var first = polys.First();
        var manager = data.Models.Vegetation;
        var mb = new MeshBuilder();
        float areaPerMark = 100f;
        float markSize = 10f;
        for (var i = 0; i < polys.Count; i++)
        {
            var p = polys[i];
            var offset = first.GetOffsetTo(p, data);

            for (var j = manager.ByPriority.Count - 1; j >= 0; j--)
            {
                var aspect = manager.ByPriority[j];
                if (aspect.Ground == false) continue;
                var aspectTris = data.Planet.TerrainTris.ByName[aspect.Name].GetPolyTris(p);
                if (aspectTris == null) continue;
                aspectTris.ForEach(t => mb.AddTri(t.Transpose(offset), aspect.Color));
            }



            for (var j = manager.ByPriority.Count - 1; j >= 0; j--)
            {
                var aspect = manager.ByPriority[j];
                if (aspect.Ground) continue;
                var aspectTris = data.Planet.TerrainTris.ByName[aspect.Name].GetPolyTris(p);
                if (aspectTris == null) continue;
                aspectTris.ForEach(t =>
                {
                    var size = t.GetArea();
                    var numMarkers = Mathf.CeilToInt(size / areaPerMark);
                    for (int k = 0; k < numMarkers; k++)
                    {
                        var point = t.GetRandomPointInside(.1f, .9f);
                        var markTri = new Triangle(point + Vector2.Left * markSize / 2f + offset,
                            point + Vector2.Right * markSize / 2f  + offset,
                            point + Vector2.Up * markSize  + offset);
                        mb.AddTri(markTri, aspect.Color);
                    }
                });
            }
        }

        if (mb.Tris.Count == 0) return;
        var mesh = mb.GetMeshInstance();
        AddChild(mesh);
    }
}