
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolygonIconsChunkGraphic : Node2D
{
    public PolygonIconsChunkGraphic(MapChunk chunk, Data data, Func<MapPolygon, IEnumerable<Icon>> getIcons)
    {
        var iconDic = new Dictionary<Icon, List<Vector2>>();
        foreach (var p in chunk.Polys)
        {
            var offset = chunk.RelTo.GetOffsetTo(p, data);
            var icons = getIcons(p);
            if (icons == null) continue;
            var numIcons = icons.Count();
            int iter = 0;
            var step = 30f;
            foreach (var icon in icons)
            {
                var iconPos = offset + step * numIcons * Vector2.Left / 2f + step * iter * Vector2.Right;
                iter++;
                iconDic.AddOrUpdate(icon, iconPos);
            }
        }
        foreach (var kvp in iconDic)
        {
            var icon = kvp.Key;
            var poses = kvp.Value;
            var mmi = new MultiMeshInstance2D();
            var mm = new MultiMesh();
            mm.Mesh = icon.Mesh;
            mm.InstanceCount = poses.Count;
            for (var i = 0; i < poses.Count; i++)
            {
                var transform = new Transform2D(Vector2.Right, Vector2.Up, poses[i]);
                mm.SetInstanceTransform2d(i, transform);
            }
            mmi.Texture = icon.BaseTexture;
            mmi.Multimesh = mm;
            AddChild(mmi);
        }
    }
}
