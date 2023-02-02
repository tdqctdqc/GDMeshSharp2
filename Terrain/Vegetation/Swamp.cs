using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Swamp : Vegetation
{
    public Swamp() 
        : base(new HashSet<Landform>{LandformManager.Plain}, .7f, .25f, Colors.DarkOliveGreen, "Swamp",
            true,
            new BlobTriBuilder())
    {
    }

    public override bool Allowed(MapPolygon p, GenData data)
    {
        return base.Allowed(p, data) && p.Altitude < .6f && p.Roughness < .15f;
    }
}