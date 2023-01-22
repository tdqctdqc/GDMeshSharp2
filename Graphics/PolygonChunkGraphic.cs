using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolygonChunkGraphic : Node2D
{
    public void Setup(List<MapPolygon> polys, Data data, Func<MapPolygon, Color> getColor)
    {
        var first = polys.First();
        var tris = new List<Vector2>();
        var colors = new List<Color>();
        for (var i = 0; i < polys.Count; i++)
        {
            var p = polys[i];
            var color = getColor(p);
            var polyTriPoints = p.GetTrisRel(data).Select(v => v + first.GetOffsetTo(p, data.Planet.Width));
            tris.AddRange(polyTriPoints);
            for (int j = 0; j < polyTriPoints.Count() / 3; j++)
            {
                colors.Add(color);
            }
        }
        var mesh = MeshGenerator.GetMeshInstance(tris.ToList(), colors);
        AddChild(mesh);
    }
}