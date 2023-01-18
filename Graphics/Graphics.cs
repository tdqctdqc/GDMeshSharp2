using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class Graphics
{
    public static void BuildGraphics(Node node, GraphicLayerHolder holder, WorldData data)
    {
        
        

        
    }

    

    public static GraphicsSegmenter<Node2D> GetTerrainAspectTrisGraphics<T>(TerrainAspectManager<T> aspectManager, WorldData data)
        where T : TerrainAspect
    {
        var els = new List<Node2D>();
        for (var i = aspectManager.ByPriority.Count - 1; i >= 0; i--)
        {
            var aspect = aspectManager.ByPriority[i];
            var holder = aspectManager.Holders[aspect];
            foreach (var kvp2 in holder.Tris)
            {
                if (kvp2.Value.Count == 0) continue;
                var tris = new List<Vector2>();
                kvp2.Value.ForEach(tri =>
                {
                    tris.Add(tri.A);
                    tris.Add(tri.B);
                    tris.Add(tri.C);
                });
                var mesh = MeshGenerator.GetMeshInstance(tris);
                mesh.Position = kvp2.Key.Center;
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
        data.Locations.Roads.ToList().ForEach(r =>
        {
            var mesh = MeshGenerator.GetLineMesh(Vector2.Zero, r.T1.GetOffsetTo(r.T2, data.Dimensions.x), 10f);
            mesh.Position = r.T1.Center;
            mesh.Modulate = Colors.Gray;
            result.Add(mesh);
        });
        segmenter.Setup(result, 10, n => n.Position, data);
        return segmenter;
    }
}
