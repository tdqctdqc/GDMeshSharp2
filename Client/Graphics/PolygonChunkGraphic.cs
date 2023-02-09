using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolygonChunkGraphic : Node2D
{
    public void Setup(List<MapPolygon> polys, Data data, Func<MapPolygon, Color> getColor, bool labels = false)
    {
        var mb = new MeshBuilder();
        mb.AddPolysRelative(polys.First(), polys, getColor, data);
        var mesh = mb.GetMeshInstance();
        AddChild(mesh);
        if (labels) AddLabels(polys, data);
    }

    private void AddLabels(List<MapPolygon> polys, Data data)
    {
        var mb = new MeshBuilder();
        mb.AddPointMarkers(polys.Select(p => polys.First().GetOffsetTo(p, data)).ToList(), 40f, Colors.White);
        var backgrounds = mb.GetMeshInstance();
        AddChild(backgrounds);
        polys.ForEach(p =>
        {
            var n = new Node2D();
            n.Position = polys.First().GetOffsetTo(p, data);
            var label = new Label();
            label.Text = p.Id.ToString();
            label.Modulate = Colors.Black;
            n.ChildAndCenterOn(label, Vector2.One * 40f);
            AddChild(n);
        });
    }

    public void SetupWithBorder(List<MapPolygon> polys, Data data, Func<MapPolygon, Color> getColor,
        Func<MapPolygon, MapPolygon, bool> native, Func<MapPolygon, bool> ignore, float mainTransparency, 
        float borderTransparency, float borderWidth)
    {
        var first = polys.First();
        var mb = new MeshBuilder();
        var unions = UnionFind<MapPolygon>
            .DoUnionFind(
                polys, 
                native,
                p => p.Neighbors.Refs()
            );

        for (var i = 0; i < unions.Count; i++)
        {
            var union = unions[i];
            if (union.Count == 0) continue;
            var uColor = ignore(union[0]) == false
                ? new Color(getColor(union[0]), mainTransparency)
                : Colors.Transparent;
            mb.AddPolysRelative(first, union, p => uColor, data);
        }
        
        var borders = polys.SelectMany(p => p.GetNeighborBorders(data)).Distinct()
            .Where(b => native(b.HighId.Ref(), b.LowId.Ref()) == false).ToList();
        foreach (var border in borders)
        {
            mb.AddPolyBorders(first, borders, borderWidth, 
                p =>  ignore(p) == false
                    ? new Color(getColor(p), mainTransparency)
                    : Colors.Transparent,
                data);
        }
        
        var mesh = mb.GetMeshInstance();
        AddChild(mesh);
    }
}