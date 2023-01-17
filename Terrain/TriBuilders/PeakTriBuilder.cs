using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PeakTriBuilder : ITriBuilder
{
    private Func<GeoPolygon, bool> _checkNeighborStrong;

    public PeakTriBuilder(Func<GeoPolygon, bool> checkNeighborStrong)
    {
        _checkNeighborStrong = checkNeighborStrong;
    }

    public List<Triangle> BuildTrisForPoly(GeoPolygon p)
    {
        var strongNeighbors = p.GeoNeighbors.Where(n => _checkNeighborStrong(p));
                    
        var tris = strongNeighbors.SelectMany(n => p.GetPolyBorder(n).GetSegsRel(p)
            .Select(s => new Triangle(s.From * .5f, s.To * .5f, Vector2.Zero))).ToList();
        return tris;
    }
}