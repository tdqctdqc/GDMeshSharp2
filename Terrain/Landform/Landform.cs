using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Landform : TerrainAspect
{
    public override  string Name { get; protected set; }
    public float MinRoughness { get; private set; }
    public override bool Allowed(GenPolygon poly, WorldData data)
    {
        return poly.Roughness >= MinRoughness && poly.IsWater() == (this == LandformManager.Water);
    }

    public override Color Color { get; protected set; }
    public override ITriBuilder TriBuilder { get; protected set; }


    public Landform(string name, float minRoughness, Color color, ITriBuilder triBuilder)
    {
        TriBuilder = triBuilder;
        Name = name;
        MinRoughness = minRoughness;
        Color = color;
    }

}