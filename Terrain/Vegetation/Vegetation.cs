using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Vegetation : TerrainAspect
{
    public HashSet<Landform> AllowedLandforms { get; private set; }
    public float MinMoisture { get; private set; }
    public float FertilityMod { get; private set; }
    public override string Name { get; protected set; }
    public override ITriBuilder TriBuilder { get; protected set; }
    public override Color Color { get; protected set; }

    public Vegetation(HashSet<Landform> allowedLandforms, float minMoisture, float fertilityMod, 
        Color color, string name, ITriBuilder triBuilder)
    {
        FertilityMod = fertilityMod;
        TriBuilder = triBuilder;
        AllowedLandforms = allowedLandforms;
        MinMoisture = minMoisture;
        Color = color;
        Name = name;
    }

    public override bool Allowed(MapPolygon p, GenData data)
    {
        var pLandform = data.Models.Landforms.GetAspectFromPoly(p, data);
        return AllowedLandforms.Contains(pLandform) && p.Moisture >= MinMoisture;
    }
}