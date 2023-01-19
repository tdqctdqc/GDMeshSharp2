using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PeakTriBuilder : ITriBuilder
{
    private Func<GenPolygon, bool> _checkNeighborStrong;

    public PeakTriBuilder(Func<GenPolygon, bool> checkNeighborStrong)
    {
        _checkNeighborStrong = checkNeighborStrong;
    }

    public List<Triangle> BuildTrisForPoly(GenPolygon p, WorldData data)
    {
        var strongNeighbors = p.GeoNeighbors.Refs.Where(n => _checkNeighborStrong(p));
                    
        var tris = strongNeighbors.SelectMany(n => p.GetPolyBorder(n).GetSegsRel(p)
            .Select(s => new Triangle(s.From * .5f, s.To * .5f, Vector2.Zero))).ToList();
        return tris;
    }
}