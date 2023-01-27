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
        Polygons = new EntityRefCollection<MapPolygon>(new List<int>());
        Polygons.AddRef(seed, key.Data);
    }
    private Regime(object[] args, ServerWriteKey key) : base(args, key) {}
    private static Regime DeserializeConstructor(object[] args, ServerWriteKey key)
    {
        return new Regime(args, key);
    }
}