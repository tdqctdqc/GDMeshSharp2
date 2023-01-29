using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorNetStd;
using Godot;

public class PolygonChunkGraphic : Node2D
{
    public void Setup(List<MapPolygon> polys, Data data, Func<MapPolygon, Color> getColor)
    {
        var mb = new MeshBuilder();
        mb.AddPolysRelative(polys.First(), polys, getColor, data);
        var mesh = mb.GetMeshInstance();
        AddChild(mesh);
    }
    
    public void SetupWithBorder(List<MapPolygon> polys, Data data, Func<MapPolygon, Color> getColor,
        Func<MapPolygon, MapPolygon, bool> native, Func<MapPolygon, bool> ignore, float mainTransparency, 
        float borderTransparency, float borderWidth)
    {
        var first = polys.First();
        var mb = new MeshBuilder();
        var unions = UnionFind<MapPolygon, int>
            .DoUnionFind(
                polys, 
                native,
                p => p.Neighbors.Refs()
            );

        for (var i = 0; i < unions.Count; i++)
        {
            var union = unions[i];
            if (union.Count == 0) continue;
            var uColor = ignore(union[0]) == false
                ? new Color(getColor(union[0]), mainTransparency)
                : Colors.Transparent;
            mb.AddPolysRelative(first, union, p => uColor, data);
        }
        
        var borders = polys.SelectMany(p => p.GetNeighborBorders(data)).Distinct()
            .Where(b => native(b.HighId.Ref(), b.LowId.Ref()) == false).ToList();
        foreach (var border in borders)
        {
            mb.AddPolyBorders(first, borders, borderWidth, 
                p =>  ignore(p) == false
                    ? new Color(getColor(p), mainTransparency)
                    : Colors.Transparent,
                data);
        }
        
        var mesh = mb.GetMeshInstance();
        AddChild(mesh);
    }
}