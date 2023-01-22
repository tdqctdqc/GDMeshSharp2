using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CenterTriBuilder : ITriBuilder
{
    private Func<MapPolygon, float> _getRatio;

    public CenterTriBuilder(Func<MapPolygon, float> getRatio)
    {
        _getRatio = getRatio;
    }

    public List<Triangle> BuildTrisForPoly(MapPolygon p, WorldData data)
    {
        var ratio = _getRatio(p);
        var tris = new List<Triangle>();
        var segs = p.Neighbors.Refs().SelectMany(n => p.GetBorder(n, data).GetSegsRel(p));
        foreach (var seg in segs)
        {
            tris.Add(new Triangle(Vector2.Zero, seg.From * ratio, seg.To * ratio));
        }

        return tris;
    }
}