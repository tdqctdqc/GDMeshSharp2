
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public class TriIconChunkGraphic : Node2D
{
    private List<MultiMeshInstance2D> _mmis;
    private float _zoomCutoff;

    public static TriIconChunkGraphic Create<T>(MapChunk chunk, Data data,
        Func<MapPolygon, IEnumerable<T>> getEls, Func<T, PolyTri> getTri, Func<T, Icon> getIcon,
        float zoomCutoff)
    {
        var g = new TriIconChunkGraphic();
        g.Construct<T>(chunk, data, getEls, getTri, getIcon, zoomCutoff);
        return g;
    }

    private void Construct<T>(MapChunk chunk, Data data, 
        Func<MapPolygon, IEnumerable<T>> getEls, Func<T, PolyTri> getTri, Func<T, Icon> getIcon,
        float zoomCutoff)
    {
        _zoomCutoff = zoomCutoff;
        var iconDic = new Dictionary<Icon, List<Vector2>>();
        _mmis = new List<MultiMeshInstance2D>();
        foreach (var p in chunk.Polys)
        {
            var offset = chunk.RelTo.GetOffsetTo(p, data);
            var els = getEls(p);
            if (els == null) continue;
            foreach (var e in els)
            {
                var icon = getIcon(e);
                var t = getTri(e);
                iconDic.AddOrUpdate(icon, offset + t.GetCentroid());
            }
        }
        foreach (var kvp in iconDic)
        {
            var icon = kvp.Key;
            var poses = kvp.Value;
            var mmi = new MultiMeshInstance2D();
            _mmis.Add(mmi);
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

    public override void _Process(float delta)
    {
        var zoom = Game.I.Client.Cam.ZoomOut;
        if (zoom > _zoomCutoff)
        {
            Visible = false;
        }
        else
        {
            Visible = true;
            zoom = Mathf.Max(1f, zoom);
            foreach (var mmi in _mmis)
            {
                var mm = mmi.Multimesh;
                for (var i = 0; i < mm.InstanceCount; i++)
                {
                    var pos = mm.GetInstanceTransform2d(i).origin;
                    mm.SetInstanceTransform2d(i, 
                        new Transform2D(Vector2.Right * zoom, Vector2.Up * zoom, pos));
                }
            }
        }
    }
}
