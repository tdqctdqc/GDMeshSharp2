
using System.Linq;
using System.Collections.Generic;
using Godot;
using MessagePack;

public class ResourceProdProcedure : Procedure
{
    public Dictionary<EntityRef<Regime>, Dictionary<Resource, float>> RegimeResourceGains { get; private set; }
    public Dictionary<EntityRef<ResourceDeposit>, float> Depletions { get; private set; }

    public static ResourceProdProcedure Create(ProductionResult prodResult)
    {
        var regimeGains = prodResult.Production.ToDictionary(kvp => kvp.Key.MakeRef(), kvp => kvp.Value);
        var depositDepletions = prodResult.Depletions.ToDictionary(kvp => kvp.Key.MakeRef(), kvp => kvp.Value);
        return new ResourceProdProcedure(regimeGains, depositDepletions);
    }
    [SerializationConstructor] private ResourceProdProcedure(
        Dictionary<EntityRef<Regime>, Dictionary<Resource, float>> regimeResourceGains, 
        Dictionary<EntityRef<ResourceDeposit>, float> depletions)
    {
        RegimeResourceGains = regimeResourceGains;
        Depletions = depletions;
    }

    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        foreach (var kvp in RegimeResourceGains)
        {
            var r = kvp.Key.Entity();
            var gains = kvp.Value;
            foreach (var kvp2 in gains)
            {
                r.Resources.Add(kvp2.Key, kvp2.Value, key);
            }
        }

        foreach (var kvp in Depletions)
        {
            var deposit = kvp.Key.Entity();
            var loss = kvp.Value;
            var newSize = Mathf.Max(0f, deposit.Size - loss);
            deposit.Set(nameof(deposit.Size), newSize, key);
        }
    }
}
