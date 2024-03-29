using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapChunk
{
    public Vector2 Coords { get; private set; }
    public HashSet<MapPolygon> Polys { get; private set; }
    public MapPolygon RelTo { get; private set; }
    public Color Color { get; private set; }
    
    public MapChunk(IEnumerable<MapPolygon> polys, Vector2 coords)
    {
        Coords = coords;
        Polys = polys.ToHashSet();
        RelTo = polys.First();
        Color = ColorsExt.GetRandomColor();
    }
}