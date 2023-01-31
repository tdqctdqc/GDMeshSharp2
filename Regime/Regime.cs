using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Regime : Entity
{
    public Regime(int id) : base(id)
    {
    }

    public Color PrimaryColor { get; private set; }
    public Color SecondaryColor { get; private set; }

    public string Name { get; private set; }
    // public RegimeTerritory Territory { get; private set; }
    public EntityRefCollection<MapPolygon> Polygons { get; private set; }


    public Regime(int id, string name, Color primaryColor, Color secondaryColor, 
        EntityRefCollection<MapPolygon> polygons) : base(id)
    {
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        Polygons = polygons;
        Name = name;
    }

    public Regime(int id, string name, CreateWriteKey key, Color primaryColor, Color secondaryColor, MapPolygon seed)
    : base(id, key)
    {
        Name = name;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        Polygons = new EntityRefCollection<MapPolygon>(new List<int>());
        Polygons.AddRef(seed, key.Data);
    }
}