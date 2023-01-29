using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Godot;

public sealed class Regime : Entity
{
    public Color PrimaryColor { get; private set; }
    public Color SecondaryColor { get; private set; }
    // public RegimeTerritory Territory { get; private set; }
    public EntityRefCollection<MapPolygon> Polygons { get; private set; }


    [JsonConstructor] public Regime(int id, Color primaryColor, Color secondaryColor, EntityRefCollection<MapPolygon> polygons) : base(id)
    {
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        Polygons = polygons;
    }

    public Regime(int id, CreateWriteKey key, Color primaryColor, Color secondaryColor, MapPolygon seed)
    : base(id, key)
    {
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        Polygons = new EntityRefCollection<MapPolygon>(new List<int>());
        Polygons.AddRef(seed, key.Data);
    }
}