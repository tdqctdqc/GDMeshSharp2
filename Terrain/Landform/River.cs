using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class River : Landform, IDecaledTerrain
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

    public void GetDecal(MeshBuilder mb, PolyTri pt, Vector2 offset)
    {
        mb.AddTri(pt.Transpose(offset), Colors.Blue);
    }
}