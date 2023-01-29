using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PeakTriBuilder : ITriBuilder
{
    private Func<MapPolygon, bool> _checkNeighborStrong;

    public PeakTriBuilder(Func<MapPolygon, bool> checkNeighborStrong)
    {
        _checkNeighborStrong = checkNeighborStrong;
    }

    public List<Triangle> BuildTrisForPoly(MapPolygon p, GenData data)
    {
        var strongNeighbors = p.Neighbors.Refs().Where(n => _checkNeighborStrong(p));
                    
        var tris = strongNeighbors.SelectMany(n => p.GetBorder(n, data).GetSegsRel(p)
            .Select(s => new Triangle(s.From * .5f, s.To * .5f, Vector2.Zero))).ToList();
        return tris;
    }
}