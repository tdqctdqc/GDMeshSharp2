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
                (p1, p2, d) => p1.GetBorder(p2, d).MoistureFlow,
                _threshold,
                s => Mathf.Pow(s, .5f)  + 5f))
    {
        
    }

    public override bool Allowed(MapPolygon p, GenData data)
    {
        return p.IsLand() && p.Neighbors.Refs().Any(n => p.GetBorder(n, data).MoistureFlow > _threshold);
    }
}