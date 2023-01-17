using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class River : Landform
{
    private static float _threshold = 25f;
    private static float _widthRatio = .05f;

    public River()
        : base("River", 0f, Colors.Blue,
            new EdgeToTriBuilder(
                (p1, p2) => p1.GetGeoPolyBorder(p2).MoistureFlow,
                _threshold,
                s => Mathf.Pow(s, .5f)  + 5f))
    {
        
    }

    public override bool Allowed(GeoPolygon p)
    {
        return p.IsLand && p.GeoNeighbors.Any(n => p.GetGeoPolyBorder(n).MoistureFlow > _threshold);
    }
}