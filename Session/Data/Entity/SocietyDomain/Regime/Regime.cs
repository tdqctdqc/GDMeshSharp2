using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Regime : Entity
{
    public override Type GetDomainType() => typeof(SocietyDomain);
    public EntityRef<MapPolygon> Capital { get; private set; }
    public Color PrimaryColor { get; private set; }
    public Color SecondaryColor { get; private set; }
    public Wallet<Resource> Resources { get; private set; }
    public string Name { get; private set; }
    public EntityRefCollection<MapPolygon> Polygons { get; private set; }


    [SerializationConstructor] private Regime(int id, string name, Color primaryColor, Color secondaryColor, 
        EntityRefCollection<MapPolygon> polygons, EntityRef<MapPolygon> capital,
        Wallet<Resource> resources) : base(id)
    {
        Resources = resources;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        Polygons = polygons;
        Name = name;
        Capital = capital;
    }

    public static Regime Create(string name, Color primaryColor, Color secondaryColor, 
        MapPolygon seed, CreateWriteKey key)
    {
        var id = key.IdDispenser;
        var polygons = new EntityRefCollection<MapPolygon>(new HashSet<int>{seed.Id});
        var r = new Regime(id.GetID(), name, primaryColor, secondaryColor, polygons, new EntityRef<MapPolygon>(seed.Id),
            new Wallet<Resource>(new Dictionary<Resource, float>()));
        key.Create(r);
        seed.SetRegime(r, key);
        
        var regimes = key.Data.Society.Regimes.Entities;
        foreach (var regime in regimes)
        {
            if (regime != r)
            {
                RegimeRelation.Create(id.GetID(), new EntityRef<Regime>(r.Id), 
                    new EntityRef<Regime>(regime.Id), key);
            }
        }
        
        return r;
    }

    public override string ToString() => Name;
}