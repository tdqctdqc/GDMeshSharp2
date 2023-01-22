using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CellGraphic : Node2D
{
    public CellGraphic()
    {
        
    }

    public CellGraphic(Data data, GenCell genCell, Dictionary<MapPolygon, GenCell> polyCells, Color? color = null)
    {
        var tris = new List<Vector2>();
        foreach (var poly in genCell.PolyGeos)
        {
            tris.AddRange(poly.GetTrisAbs(data));
        }
        
        var triMesh = MeshGenerator.GetMeshInstance(tris);
        if (color == null) color = ColorsExt.GetRandomColor();
        triMesh.Modulate = color.Value;
        AddChild(triMesh);
    }

    
}