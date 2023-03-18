
using System.Linq;
using System.Collections.Generic;
using Godot;
using MessagePack;

public class ProdProcedure : Procedure
{
    public Dictionary<EntityRef<Regime>, Dictionary<ModelRef<StratResource>, float>> RegimeResourceGains { get; private set; }
    public Dictionary<EntityRef<ResourceDeposit>, float> Depletions { get; private set; }

    public static ProdProcedure Create(ProductionResult prodResult)
    {
        var regimeGains = prodResult.Production.ToDictionary(kvp => kvp.Key.MakeRef(), 
            kvp => kvp.Value.ToDictionary(kvp2 => kvp2.Key.MakeRef(), kvp2 => kvp2.Value));
        var depositDepletions = prodResult.Depletions.ToDictionary(kvp => kvp.Key.MakeRef(), kvp => kvp.Value);
        return new ProdProcedure(regimeGains, depositDepletions);
    }
    [SerializationConstructor] private ProdProcedure(
        Dictionary<EntityRef<Regime>, Dictionary<ModelRef<StratResource>, float>> regimeResourceGains, 
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
                r.Resources.Add(kvp2.Key.Model(), kvp2.Value, key);
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
