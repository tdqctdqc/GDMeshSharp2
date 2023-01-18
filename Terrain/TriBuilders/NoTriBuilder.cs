using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class NoTriBuilder : ITriBuilder
{
    public List<Triangle> BuildTrisForPoly(GeoPolygon p, WorldData data)
    {
        return null;
    }
}