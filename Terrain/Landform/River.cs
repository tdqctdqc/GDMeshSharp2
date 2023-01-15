using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class River : Landform
{
    public River() 
        : base("River", 0f, Colors.LightBlue, new NoTriBuilder())
    {
        
    }

    public override bool Allowed(GeologyPolygon p)
    {
        return p.IsLand && p.GeoNeighbors.Any(n => p.GetGeoPolyBorder(n).RiverWidth > 0f);
    }
}