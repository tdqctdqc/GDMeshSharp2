using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CellGraphic : Node2D
{
    public CellGraphic()
    {
        
    }

    public CellGraphic(GeoCell geoCell, Color? color = null)
    {
        var tris = new List<Vector2>();
        foreach (var poly in geoCell.PolyGeos)
        {
            tris.AddRange(poly.GetTrisAbs());
        }
        
        var triMesh = MeshGenerator.GetMeshInstance(tris);
        if (color == null) color = ColorsExt.GetRandomColor();
        triMesh.Modulate = color.Value;
        AddChild(triMesh);
        if(geoCell.Seed.Id % 25 == 0) AddOuterBorder(geoCell, color.Value);
    }

    private void AddInnerBorder(GeoCell cell, Color color)
    {
        var borderPolyGeos = cell.PolyGeos
            .Where(p => p.Neighbors.Any(n => ((GeoPolygon) n).Cell != cell));
        var iter = 0;

        foreach (var poly in borderPolyGeos)
        {
            var segments = poly.GetNeighborBorders().SelectMany(b => b.GetSegsRel(poly));
            foreach (var lineSegment in segments)
            {
                var point = lineSegment.From * .9f + poly.Center;
                var next = lineSegment.To * .9f + poly.Center;
                var arrow = MeshGenerator.GetArrowGraphic(point, next, 10f);
                arrow.Modulate = ColorsExt.GetRainbowColor(iter);
                AddChild(arrow);
            }

            iter++;
        }
        
    }
    private void AddOuterBorder(GeoCell geoCell, Color color)
    {
        var edges = geoCell.GetOrderedBorderPairs();
        var iter = 0;
        var currOppCell = edges[0].Foreign.Cell;
        for (var i = 0; i < edges.Count; i++)
        {
            var edge = edges[i];
            if (currOppCell != edges[i].Foreign.Cell)
            {
                currOppCell = edges[i].Foreign.Cell;
                iter++;
            }

            var segments = edge.Foreign.GetPolyBorder(edge.Native).GetSegsRel(edge.Foreign);
            foreach (var lineSegment in segments)
            {
                var from = lineSegment.To * 1.1f + edge.Foreign.Center;
                var to = lineSegment.From * 1.1f + edge.Foreign.Center;
                var arrow = MeshGenerator.GetArrowGraphic(from, to, 10f);
                arrow.Modulate = ColorsExt.GetRainbowColor(iter);
                AddChild(arrow);
            }
        }
        
    }
}