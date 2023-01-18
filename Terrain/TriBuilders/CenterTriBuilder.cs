using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CenterTriBuilder : ITriBuilder
{
    private Func<GeoPolygon, float> _getRatio;

    public CenterTriBuilder(Func<GeoPolygon, float> getRatio)
    {
        _getRatio = getRatio;
    }

    public List<Triangle> BuildTrisForPoly(GeoPolygon p, WorldData data)
    {
        var ratio = _getRatio(p);
        var tris = new List<Triangle>();
        var segs = p.Neighbors.SelectMany(n => p.GetGeoPolyBorder(n).GetSegsRel(p));
        foreach (var seg in segs)
        {
            tris.Add(new Triangle(Vector2.Zero, seg.From * ratio, seg.To * ratio));
        }

        return tris;
    }
}