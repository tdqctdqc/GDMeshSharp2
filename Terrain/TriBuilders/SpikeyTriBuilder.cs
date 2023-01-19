using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SpikeyTriBuilder : ITriBuilder
{
    private Func<GenPolygon, bool> _checkNeighborStrong;
    private Func<GenPolygon, float> _otherStrength;
    public SpikeyTriBuilder(Func<GenPolygon, bool> checkNeighborStrong, Func<GenPolygon, float> otherStrength)
    {
        _checkNeighborStrong = checkNeighborStrong;
        _otherStrength = otherStrength;
    }

    
    public List<Triangle> BuildTrisForPoly(GenPolygon p, WorldData data)
    {
        var polyTris = new List<Triangle>();
        p.GeoNeighbors.ForEach(n =>
        {
            var segs = p.GetPolyBorder(n)
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
                var otherStrength = Mathf.Clamp(_otherStrength(n), 0f, .8f);
                var spike = segs.GetMiddlePoint() * otherStrength;
                var firstLeg = segs[0].From * .5f * otherStrength;
                var lastLeg = segs[segs.Count - 1].To * .5f * otherStrength;
                polyTris.Add(new Triangle(firstLeg, lastLeg, spike));
                polyTris.Add(new Triangle(firstLeg, lastLeg, Vector2.Zero));
            }
        });
        return polyTris;
    }
}