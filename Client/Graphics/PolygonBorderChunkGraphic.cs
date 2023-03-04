
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolygonBorderChunkGraphic : Node2D
{
    public void SetupForRegime(MapPolygon relTo, List<MapPolygon> polys, float thickness,
        Data data)
    {
        if (polys.Count == 0) return;
        var mb = new MeshBuilder();

        var borders = polys.SelectMany(p => p.GetNeighborBorders(data))
            .Distinct()
            .Where(b => b.LowId.Entity().Regime.RefId != b.HighId.Entity().Regime.RefId)
            .ToList();
        mb.AddPolyBorders(relTo, borders, thickness, 
            p => p.Regime.Empty() 
                ? Colors.Transparent 
                : new Color(p.Regime.Entity().PrimaryColor, .5f),
            data
        );
        if (mb.Tris.Count == 0) return;
        AddChild(mb.GetMeshInstance());
    }
}
