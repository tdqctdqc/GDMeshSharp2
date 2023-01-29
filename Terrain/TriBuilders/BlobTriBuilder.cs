using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BlobTriBuilder : ITriBuilder
{
    public List<Triangle> BuildTrisForPoly(MapPolygon p, GenData data)
    {
        var borders = data.Planet.PolyBorders;
        return p.Neighbors.Refs().SelectMany(n => borders.GetBorder(p, n).GetSegsRel(p)
            .Select(s => new Triangle(s.From, s.To, Vector2.Zero))).ToList();
    }
}