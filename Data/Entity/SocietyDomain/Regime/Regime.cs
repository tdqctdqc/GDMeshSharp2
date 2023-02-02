using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Regime : Entity
{
    public override Type GetDomainType() => typeof(SocietyDomain);

    public Color PrimaryColor { get; private set; }
    public Color SecondaryColor { get; private set; }

    public string Name { get; private set; }
    // public RegimeTerritory Territory { get; private set; }
    public EntityRefCollection<MapPolygon> Polygons { get; private set; }


    [SerializationConstructor] private Regime(int id, string name, Color primaryColor, Color secondaryColor, 
        EntityRefCollection<MapPolygon> polygons) : base(id)
    {
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        Polygons = polygons;
        Name = name;
    }

    public static Regime Create(int id, string name, Color primaryColor, Color secondaryColor, 
        MapPolygon seed, CreateWriteKey key)
    {
        var polygons = new EntityRefCollection<MapPolygon>(new HashSet<int>());
        polygons.AddRef(seed, key.Data);
        var r = new Regime(id, name, primaryColor, secondaryColor, polygons);
        key.Create(r);
        return r;
    }
}