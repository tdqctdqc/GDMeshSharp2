using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyFillChunkGraphic : Node2D
{
    private Action _update;
    public PolyFillChunkGraphic(MapChunk chunk, Data data, Func<MapPolygon, Color> getColor, 
        float transparency = 1f, 
        Action<PolyFillChunkGraphic> update = null)
    {
        _update = () => update(this);
        var mb = new MeshBuilder();
        mb.AddPolysRelative(chunk.RelTo, chunk.Polys, getColor, data);
        var mesh = mb.GetMeshInstance();
        AddChild(mesh);
        Modulate = new Color(Colors.Transparent, transparency);
    }

    private PolyFillChunkGraphic()
    {
        
    }

    public override void _Process(float delta)
    {
        _update?.Invoke();
    }

    private void AddLabels(IEnumerable<MapPolygon> polys, Data data)
    {
        var mb = new MeshBuilder();
        mb.AddPointMarkers(polys.Select(p => polys.First().GetOffsetTo(p, data)).ToList(), 
            40f, Colors.White);
        var backgrounds = mb.GetMeshInstance();
        AddChild(backgrounds);
        foreach (var p in polys)
        {
            var n = new Node2D();
            n.Position = polys.First().GetOffsetTo(p, data);
            var label = new Label();
            label.Text = p.Id.ToString();
            label.Modulate = Colors.Black;
            n.ChildAndCenterOn(label, Vector2.One * 40f);
            AddChild(n);
        }
    }

}