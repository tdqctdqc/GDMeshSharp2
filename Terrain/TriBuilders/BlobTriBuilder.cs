using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BlobTriBuilder : ITriBuilder
{
    public List<Triangle> BuildTrisForPoly(GenPolygon p, WorldData data)
    {
        return p.GeoNeighbors.SelectMany(n => p.GetPolyBorder(n).GetSegsRel(p)
            .Select(s => new Triangle(s.From, s.To, Vector2.Zero))).ToList();
    }
}