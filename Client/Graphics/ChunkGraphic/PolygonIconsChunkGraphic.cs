
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolygonIconsChunkGraphic : Node2D
{
    public PolygonIconsChunkGraphic(MapChunk chunk, Data data, 
        Func<MapPolygon, IEnumerable<IEnumerable<Icon>>> getIconGroups, int stackMin)
    {
        var iconDic = new Dictionary<Icon, List<Vector2>>();
        foreach (var p in chunk.Polys)
        {
            var polyOffset = chunk.RelTo.GetOffsetTo(p, data);
            var iconGroups = getIconGroups(p);
            var yOffset = Vector2.Zero;
            foreach (var icons in iconGroups)
            {
                if (icons == null) continue;
                var iconCounts = icons.GetCounts();
                var numIcons = icons.Count();
                
                int iter = 0;
                var step = 5f;
                foreach (var icon in icons)
                {
                    var iconPos = polyOffset + yOffset + step * numIcons * Vector2.Left / 2f + step * iter * Vector2.Right;
                    iter++;
                    iconDic.AddOrUpdate(icon, iconPos);
                }

                yOffset += Vector2.Down * icons.First().Dimension.y;
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
