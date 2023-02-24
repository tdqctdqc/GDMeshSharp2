using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class River : Landform
{
    public static readonly float WidthFloor = 5f, 
        WidthCeil = 30f,
        FlowFloor = 1f,
        FlowCeil = 200f;
    public River()
        : base("River", Mathf.Inf, 0f, Colors.Blue)
    {
        
    }

    public override bool Allowed(MapPolygon p, PolyTri t, GenData data)
    {
        return false;
    }
}