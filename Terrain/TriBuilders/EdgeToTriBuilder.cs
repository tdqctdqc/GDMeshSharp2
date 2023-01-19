using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class EdgeToTriBuilder : ITriBuilder
{
    private Func<GenPolygon, GenPolygon, float> _getEdgeStrength;
    private float _threshhold;
    private Func<float, float> _widthFromStrength;

    public EdgeToTriBuilder(Func<GenPolygon, GenPolygon, float> getEdgeStrength, float threshhold, Func<float, float> widthFromStrength)
    {
        _getEdgeStrength = getEdgeStrength;
        _threshhold = threshhold;
        _widthFromStrength = widthFromStrength;
    }

    public List<Triangle> BuildTrisForPoly(GenPolygon p, WorldData data)
    {
        var tris = new List<Triangle>();
        
        foreach (var n in p.GeoNeighbors.Refs)
        {
            var strength = _getEdgeStrength(p, n);
            if (strength >= _threshhold)
            {
                var segs = p.GetGeoPolyBorder(n).GetSegsRel(p);
                var width = _widthFromStrength(strength);
                var axis = p.GetOffsetTo(n, data.Dimensions.x);
                var axisRot = axis.Rotated(Mathf.Pi / 2f).Normalized();
                var bl = axisRot * width / 2f;
                var br = -bl;

                var tl = bl + axis;
                var tr = br + axis;
                tris.Add(new Triangle(tl, br, bl));
                tris.Add(new Triangle(tl, tr, br));
            }
        }
        return tris;
    }
}