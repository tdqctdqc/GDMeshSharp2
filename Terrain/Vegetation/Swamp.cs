using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Swamp : Vegetation
{
    public Swamp() 
        : base(new HashSet<Landform>{LandformManager.Plain}, .7f, Colors.DarkOliveGreen, "Swamp", new BlobTriBuilder())
    {
    }

    public override bool Allowed(GeoPolygon p, WorldData data)
    {
        return base.Allowed(p, data) && p.Altitude < .6f;
    }
}