using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapChunk
{
    public HashSet<MapPolygon> Polys { get; private set; }
    public MapPolygon RelTo { get; private set; }
    public Color Color { get; private set; }
    
    public MapChunk(IEnumerable<MapPolygon> polys)
    {
        Polys = polys.ToHashSet();
        RelTo = polys.First();
        Color = ColorsExt.GetRandomColor();
    }
}