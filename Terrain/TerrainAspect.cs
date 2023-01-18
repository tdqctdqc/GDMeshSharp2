using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Godot;

public abstract class TerrainAspect
{
    public abstract string Name { get; protected set; }
    public abstract ITriBuilder TriBuilder { get; protected set; }
    public abstract bool Allowed(GeoPolygon poly, WorldData data);
    public abstract Color Color { get; protected set; }
    // public abstract bool Passes(GeologyPolygon p);
}