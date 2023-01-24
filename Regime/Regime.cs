using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public sealed class Regime : Entity
{
    public Color PrimaryColor { get; private set; }
    public Color SecondaryColor { get; private set; }
    // public RegimeTerritory Territory { get; private set; }
    public EntityRefCollection<MapPolygon> Polygons { get; private set; }
    public Regime(int id, CreateWriteKey key, Color primaryColor, Color secondaryColor, MapPolygon seed)
    : base(id, key)
    {
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        Polygons = new EntityRefCollection<MapPolygon>(new int[0]);
        Polygons.AddRef(seed, key.Data);
    }
    private Regime(string json) : base(json) {}
    private static Regime DeserializeConstructor(string json)
    {
        return new Regime(json);
    }
}