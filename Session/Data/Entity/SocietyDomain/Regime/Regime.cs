using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Regime : Entity
{
    public EntityRef<MapPolygon> Capital { get; protected set; }
    public ModelRef<Culture> Culture { get; private set; }
    public ModelRef<RegimeTemplate> Template { get; private set; }
    public Color PrimaryColor { get; protected set; }
    public Color SecondaryColor { get; protected set; }
    public ItemWallet Items { get; protected set; }
    public RegimeHistory History { get; private set; }
    public string Name { get; protected set; }
    public EntityRefCollection<MapPolygon> Polygons { get; protected set; }
    public RegimeFinance Finance { get; private set; }

    [SerializationConstructor] private Regime(int id, string name, Color primaryColor, Color secondaryColor, 
        EntityRefCollection<MapPolygon> polygons, EntityRef<MapPolygon> capital,
        ItemWallet items, RegimeHistory history, ModelRef<Culture> culture,
        ModelRef<RegimeTemplate> template, RegimeFinance finance) : base(id)
    {
        Items = items;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        Polygons = polygons;
        Name = name;
        Capital = capital;
        History = history;
        Culture = culture;
        Template = template;
        Finance = finance;
    }

    public static Regime Create(MapPolygon seed, RegimeTemplate regimeTemplate, CreateWriteKey key)
    {
        var id = key.IdDispenser;
        var polygons = EntityRefCollection<MapPolygon>.Construct(new HashSet<int>{seed.Id}, key.Data);
        var r = new Regime(id.GetID(), regimeTemplate.Name, 
            new Color(regimeTemplate.PrimaryColor), 
            new Color(regimeTemplate.SecondaryColor), 
            polygons, new EntityRef<MapPolygon>(seed.Id),
            ItemWallet.Construct(), RegimeHistory.Construct(key.Data), 
            regimeTemplate.Culture.MakeRef(),
            regimeTemplate.MakeRef(),
            RegimeFinance.Construct()
        );
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
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(SocietyDomain);
}