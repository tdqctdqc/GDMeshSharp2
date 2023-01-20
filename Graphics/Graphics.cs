using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class Graphics
{
    public static void BuildGraphics(Node node, GraphicLayerHolder holder, WorldData data)
    {
        
        

        
    }

    

    public static GraphicsSegmenter<Node2D> GetTerrainAspectTrisGraphics<T>(TerrainAspectManager<T> aspectManager, 
        WorldData data)
        where T : TerrainAspect
    {
        var els = new List<Node2D>();
        for (var i = aspectManager.ByPriority.Count - 1; i >= 0; i--)
        {
            var aspect = aspectManager.ByPriority[i];
            var holder = data.PlanetDomain.TerrainTris.GetTris(aspect);
            foreach (var kvp2 in holder.Tris)
            {
                var poly = (GenPolygon)data[kvp2.Key];
                if (kvp2.Value.Count == 0) continue;
                var tris = new List<Vector2>();
                kvp2.Value.ForEach(tri =>
                {
                    tris.Add(tri.A);
                    tris.Add(tri.B);
                    tris.Add(tri.C);
                });
                var mesh = MeshGenerator.GetMeshInstance(tris);
                mesh.Position = poly.Center;
                mesh.Modulate = aspect.Color;
                els.Add(mesh);
            }
        }
        var res = new GraphicsSegmenter<Node2D>();
        res.Setup(els, 10, e => e.Position, data);
        return res;
    }

    public static GraphicsSegmenter<Node2D> RoadGraphics(WorldData data)
    {
        var segmenter = new GraphicsSegmenter<Node2D>();
        var result = new List<Node2D>();
        foreach (var r in data.SocietyDomain.Roads.Entities)
        {
            var mesh = MeshGenerator.GetLineMesh(Vector2.Zero, r.P1.Ref.GetOffsetTo(r.P2.Ref, data.Dimensions.x), 10f);
            mesh.Position = r.P1.Ref.Center;
            mesh.Modulate = Colors.Gray;
            result.Add(mesh);
        }
        segmenter.Setup(result, 10, n => n.Position, data);
        return segmenter;
    }
}
