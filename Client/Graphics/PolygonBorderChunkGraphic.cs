
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolygonBorderChunkGraphic : Node2D
{
    public void SetupForRegime(MapPolygon relTo, List<MapPolygon> polys, float thickness,
        Data data)
    {
        var mb = new MeshBuilder();
        var regPolys = polys.Where(p => p.Regime.Empty() == false);
        foreach (var p in regPolys)
        {
            var color = p.Regime.Entity().PrimaryColor.Darkened(.2f);
            var offset = relTo.GetOffsetTo(p, data);
            foreach (var n in p.Neighbors.Refs())
            {
                
                if (n.Regime.RefId == p.Regime.RefId) continue;
                
                mb.DrawMapPolyEdge(p, p.GetEdge(n, data), data, 20f, color, offset);
            }
        }
        
        if (mb.Tris.Count == 0) return;
        AddChild(mb.GetMeshInstance());
    }
}
