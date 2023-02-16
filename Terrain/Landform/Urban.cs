using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Urban : Landform
{
    
    public Urban() 
        : base("Urban", 1000f, 0f, Colors.Black)
    {
        
    }

    public override bool Allowed(MapPolygon poly, PolyTri t, GenData data)
    {
        return false;
    }

    private static float GetSize(MapPolygon p)
    {
        return Mathf.Clamp(Mathf.Sqrt(p.SettlementSize) * .2f, 0f, 1f);
    }
}