using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class River : Landform
{
    public static readonly float WidthFloor = 10f, 
        WidthCeil = 30f,
        FlowFloor = 1f,
        FlowCeil = 200f;
    public River()
        : base("River", Mathf.Inf, 0f, Colors.Red, true)
    {
    }

    public static float GetWidthFromFlow(float flow)
    {
        var logBase = Mathf.Pow(River.FlowCeil, 1f / (River.WidthCeil - River.WidthFloor));
        return Mathf.Max(0f, (float)Math.Log(flow, logBase));
    }
}