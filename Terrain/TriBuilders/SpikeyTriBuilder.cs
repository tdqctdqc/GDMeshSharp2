using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SpikeyTriBuilder : ITriBuilder
{
    private Func<MapPolygon, bool> _checkNeighborStrong;
    private Func<MapPolygon, float> _otherStrength;
    public SpikeyTriBuilder(Func<MapPolygon, bool> checkNeighborStrong, Func<MapPolygon, float> otherStrength)
    {
        _checkNeighborStrong = checkNeighborStrong;
        _otherStrength = otherStrength;
    }

    
    public List<Triangle> BuildTrisForPoly(MapPolygon p, GenData data)
    {
        var polyTris = new List<Triangle>();
        foreach (var n in p.Neighbors.Refs())
        {
            var segs = p.GetBorder(n, data)
                .GetSegsRel(p);
            if (_checkNeighborStrong(n))
            {
                segs.ForEach(seg =>
                {
                    polyTris.Add(new Triangle(seg.From, seg.To, Vector2.Zero));
                });
            }
            else
            {
                var otherStrength = Mathf.Clamp(_otherStrength(n), .1f, .8f);
                var spike = segs.GetMiddlePoint() * otherStrength;
                var firstLeg = segs[0].From * .5f * otherStrength;
                var lastLeg = segs[segs.Count - 1].To * .5f * otherStrength;
                polyTris.Add(new Triangle(firstLeg, lastLeg, spike));
                polyTris.Add(new Triangle(firstLeg, lastLeg, Vector2.Zero));
            }
        }

        return polyTris;
    }
}