using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CellGraphic : Node2D
{
    public CellGraphic()
    {
        
    }

    public CellGraphic(GeologyCell geologyCell, Color? color = null)
    {
        var tris = new List<Vector2>();
        foreach (var poly in geologyCell.PolyGeos)
        {
            tris.AddRange(poly.GetTris());
        }
        
        var triMesh = MeshGenerator.GetMeshInstance(tris);
        if (color == null) color = ColorsExt.GetRandomColor();
        triMesh.Modulate = color.Value;
        AddChild(triMesh);
        if(geologyCell.Seed.Id % 25 == 0) AddBorder(geologyCell, color.Value);
    }

    private void AddBorder(GeologyCell geologyCell, Color color)
    {
        var border = geologyCell.GetOrderedOuterBorder();
        var iter = 0;
        for (var i = 0; i < border.Count; i++)
        {
            var poly = border[i];
            var polyForeignEdges = poly.GeoNeighbors
                .Where(n => n.Cell == geologyCell)
                .Select(n => poly.GetEdge(n))
                .SelectMany(b => b.GetPointsRel(poly))
                .Distinct().Reverse().ToList();
            for (var j = 0; j < polyForeignEdges.Count - 1; j++)
            {
                var point = polyForeignEdges[j] * 1.1f + poly.Center;
                var next = polyForeignEdges[(j + 1) % polyForeignEdges.Count] * 1.1f + poly.Center;
                var arrow = MeshGenerator.GetArrowGraphic(point, next, 10f);
                arrow.Modulate = ColorsExt.GetRainbowColor(iter);
                AddChild(arrow);
            }
            iter++;
        }
    }
}