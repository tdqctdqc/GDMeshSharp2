using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class TerrainAspect : IModel
{
    public abstract string Name { get; protected set; }
    // public abstract bool Allowed(MapPolygon poly, PolyTri t, GenData data);
    public abstract Color Color { get; protected set; }
    public TerrainAspect()
    {
    }
}