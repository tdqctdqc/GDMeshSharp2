using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyHighlighter : Node2D
{
    private List<MeshInstance2D> _mis;
    public PolyHighlighter()
    {
        _mis = new List<MeshInstance2D>();
    }
    public void DrawOutline(Data data, MapPolygon poly, Vector2 offset, IClient client)
    {
        Visible = true;
        Clear();
        Move(data, client, poly);
        var mb = new MeshBuilder();
        var lines = poly.BorderSegments;
        mb.AddArrowsRainbow(lines.ToList(), 5f);
        mb.AddNumMarkers(lines.Select(ls => ls.Mid()).ToList(), 20f, Colors.Transparent);
        TakeFromMeshBuilder(mb);
    }
    private void TakeFromMeshBuilder(MeshBuilder mb)
    {
        var mi = mb.GetMeshInstance();
        mb.Clear();
        AddChild(mi);
        _mis.Add(mi);
    }
    private void Move(Data data, IClient client, MapPolygon poly)
    {
        Position = client.Cam.GetMapPosInGlobalSpace(poly.Center, data);
    }
    public void Clear()
    {
        _mis.ForEach(mi =>
        {
            RemoveChild(mi);
            mi?.QueueFree();
            mi = null;
        });
        _mis.Clear();
    }
}