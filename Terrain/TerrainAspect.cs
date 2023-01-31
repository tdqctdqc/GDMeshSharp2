using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class TerrainAspect : IModel
{
    public abstract string Name { get; protected set; }
    public abstract ITriBuilder TriBuilder { get; protected set; }
    public abstract bool Allowed(MapPolygon poly, GenData data);
    public abstract Color Color { get; protected set; }
    // public TerrainTriHolder TriHolder { get; private set; }

    public TerrainAspect()
    {
        // TriHolder = new TerrainTriHolder();
    }
}