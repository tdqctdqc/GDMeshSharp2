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
    public void SetupWheel(List<MapPolygon> polys, Data data, Func<int, Color> getColor, bool labels = false)
    {
        var mb = new MeshBuilder();
        
        
        mb.AddPolyWheelTrisRelative(polys.First(), polys, getColor, data);
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

}