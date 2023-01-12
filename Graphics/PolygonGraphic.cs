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
        triMesh.Modulate = new Color(color.Value, .5f);
        AddChild(triMesh);
        if(poly.Id % 25 == 0)
        {
            AddBorderGraphic(poly);
            // AddBorderPolysGraphic(poly, color.Value.Inverted());
        }

    }

    private void AddBorderPolysGraphic(Polygon poly, Color color)
    {
        
        for (var i = 0; i < poly.Neighbors.Count; i++)
        {
            var edge = poly.GetEdge(poly.Neighbors[i]);
            var offset = edge.GetOffsetToOtherPoly(poly);
            var centerArrow = MeshGenerator.GetArrowGraphic(poly.Center,  poly.Center + offset, 10f);
            AddChild(centerArrow);
            


            var next = (i + 1) % poly.Neighbors.Count;
            
            var from = poly.GetEdge(poly.Neighbors[i]).GetPointsRel(poly).Avg();
            var to = poly.GetEdge(poly.Neighbors[next]).GetPointsRel(poly).Avg();
            var arrow = MeshGenerator.GetArrowGraphic(from + poly.Center, to + poly.Center, 5f);
            arrow.Modulate = color;
            AddChild(arrow);
        }
    }
    private void AddBorderGraphic(Polygon poly)
    {
        var borders = poly.NeighborBorders.ToList();
        var iter = 0;
        for (var i = 0; i < borders.Count; i++)
        {
            var border = borders[i].GetSegsRel(poly);
            for (var j = 0; j < border.Count; j++)
            {
                var seg = border[j];
                var from = seg.From * .9f + poly.Center;
                var to = seg.To * .9f + poly.Center;
                if (from == to) continue;
                var arrow = MeshGenerator.GetArrowGraphic(from, to, 5f);
                arrow.Modulate = ColorsExt.GetRainbowColor(iter);
                iter++;
                AddChild(arrow);
            }
        }
    }
}