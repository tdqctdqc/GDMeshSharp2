using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class River : Landform
{

    public River()
        : base("River", Mathf.Inf, 0f, Colors.Blue)
    {
        
    }

    public override bool Allowed(MapPolygon p, PolyTri t, GenData data)
    {
        return false;
    }
}