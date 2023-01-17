using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Urban : Landform
{
    
    public Urban() 
        : base("Urban", 0f, Colors.Black, new CenterTriBuilder(GetSize))
    {
        
    }

    public override bool Allowed(GeoPolygon poly)
    {
        return poly.SettlementSize > 0f && base.Allowed(poly);
    }

    private static float GetSize(GeoPolygon p)
    {
        return Mathf.Clamp(Mathf.Sqrt(p.SettlementSize) * .2f, 0f, 1f);
    }
}