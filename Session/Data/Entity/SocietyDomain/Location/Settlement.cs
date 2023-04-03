using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;


public class Settlement : Location
{
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(SocietyDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public EntityRef<MapPolygon> Poly { get; protected set; }
    public ModelRef<SettlementTier> Tier { get; private set; }
    public ModelRefCollection<BuildingModel> Buildings { get; private set; }
    public int Size { get; protected set; }
    public string Name { get; protected set; }
    
    public static Settlement Create(string name, MapPolygon poly, int size, CreateWriteKey key)
    {
        var tier = key.Data.Models.SettlementTiers.GetTier(size);
        var s = new Settlement(key.IdDispenser.GetID(), poly.MakeRef(), 
            size, tier.MakeRef(), ModelRefCollection<BuildingModel>.Construct(), name);
        key.Create(s);
        return s;
    }
    [SerializationConstructor] private Settlement(int id, EntityRef<MapPolygon> poly, int size,
        ModelRef<SettlementTier> tier, ModelRefCollection<BuildingModel> buildings, 
        string name) : base(id)
    {
        Tier = tier;
        Name = name;
        Poly = poly;
        Size = size;
        Buildings = buildings;
    }
    public void AddBuilding(BuildingModel b, GenWriteKey key)
    {
        Buildings.AddByProcedure(b.Name, null); //todo bad
    }
    public void AddBuilding(BuildingModel b, ProcedureWriteKey key)
    {
        Buildings.AddByProcedure(b.Name, key);
    }
}