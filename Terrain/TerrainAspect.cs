using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Godot;

public abstract class TerrainAspect
{
    public abstract ITriBuilder TriBuilder { get; protected set; }
    public abstract bool Allowed(GeologyPolygon poly);
    public abstract Color Color { get; protected set; }
    // public abstract bool Passes(GeologyPolygon p);
}