using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Vegetation : TerrainAspect
{
    public HashSet<Landform> AllowedLandforms { get; private set; }
    public float MinMoisture { get; private set; }
    public string Name { get; private set; }
    public override ITriBuilder TriBuilder { get; protected set; }
    public override Color Color { get; protected set; }

    public Vegetation(HashSet<Landform> allowedLandforms, float minMoisture, Color color, string name, ITriBuilder triBuilder)
    {
        TriBuilder = triBuilder;
        AllowedLandforms = allowedLandforms;
        MinMoisture = minMoisture;
        Color = color;
        Name = name;
    }

    public override bool Allowed(GeologyPolygon p)
    {
        var pLandform = Root.WorldData.Landforms.GetValueFromPoly(p);
        return AllowedLandforms.Contains(pLandform) && p.Moisture >= MinMoisture;
    }
}