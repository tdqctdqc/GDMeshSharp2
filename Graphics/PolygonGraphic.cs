using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolygonGraphic : Node2D
{
    public PolygonGraphic()
    {
        
    }

    public PolygonGraphic(Polygon poly, Color? color = null, bool border = false)
    {
        var tris = poly.GetTris();
        var triMesh = MeshGenerator.GetMeshInstance(tris);
        color = color.HasValue ? color.Value : ColorsExt.GetRandomColor();
        triMesh.Modulate = color.Value;
        AddChild(triMesh);
        if(border && poly.Id % 25 == 0)
        {
            AddBorderGraphic(poly);
            // AddBorderPolysGraphic(poly, color.Value.Inverted());
        }
    }

    private void AddBorderPolysGraphic(Polygon poly, Color color)
    {
        for (var i = 0; i < poly.Neighbors.Count; i++)
        {
            var next = (i + 1) % poly.Neighbors.Count;
            
            var from = poly.NeighborBorders[i].GetPointsRel(poly).Avg();
            var to = poly.NeighborBorders[next].GetPointsRel(poly).Avg();
            var arrow = MeshGenerator.GetArrowGraphic(from + poly.Center, to + poly.Center, 5f);
            arrow.Modulate = color;
            AddChild(arrow);
        }
    }
    private void AddBorderGraphic(Polygon poly)
    {
        var borders = poly.NeighborBorders;
        var iter = 0;
        for (var i = 0; i < borders.Count; i++)
        {
            var border = borders[i].GetPointsRel(poly);
            for (var j = 0; j < border.Count - 1; j++)
            {
                var p1 = border[j];
                var p2 = border[j + 1];
                var from = p1 * .9f + poly.Center;
                var to = p2 * .9f + poly.Center;
                if (from == to) continue;
                var arrow = MeshGenerator.GetArrowGraphic(from, to, 5f);
                arrow.Modulate = ColorsExt.GetRainbowColor(iter);
                iter++;
                AddChild(arrow);
            }
        }
    }
}