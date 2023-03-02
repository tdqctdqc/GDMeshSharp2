using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Landform : TerrainAspect
{
    public override string Name { get; protected set; }
    public float MinRoughness { get; private set; }
    public float FertilityMod { get; private set; }
    public float DarkenFactor { get; private set; }
    public virtual bool Allowed(MapPolygon poly, PolyTri t, GenData data)
    {
        return poly.Roughness >= MinRoughness && poly.IsWater() == (this == LandformManager.Sea);
    }
    public override Color Color { get; protected set; }
    public Landform(string name, float minRoughness, float fertilityMod, Color color, float darkenFactor = 0f)
    {
        DarkenFactor = darkenFactor;
        FertilityMod = fertilityMod;
        Name = name;
        MinRoughness = minRoughness;
        Color = color;
    }
}