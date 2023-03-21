
using System.Linq;
using System.Collections.Generic;
using Godot;
using MessagePack;

public class ProdProcedure : Procedure
{
    public Dictionary<int, ItemWallet> RegimeResourceGains { get; private set; }
    public Dictionary<int, EntityWallet<ResourceDeposit>> Depletions { get; private set; }

    public static ProdProcedure Create(Dictionary<int, ItemWallet> resourceGains, 
        Dictionary<int, EntityWallet<ResourceDeposit>> depletions)
    {
        return new ProdProcedure(resourceGains, depletions);
    }
    [SerializationConstructor] private ProdProcedure(
        Dictionary<int, ItemWallet> regimeResourceGains, 
        Dictionary<int, EntityWallet<ResourceDeposit>> depletions)
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
        var tick = key.Data.Tick;
        foreach (var kvp in RegimeResourceGains)
        {
            var r = (Regime)key.Data[kvp.Key];
            var gains = kvp.Value;
            var snapshot = gains.GetSnapshot();
            foreach (var kvp2 in gains.Contents)
            {
                var item = key.Data.Models.Items.Models[kvp2.Key];
                r.Resources.Add(item, kvp2.Value);
            }
            r.ProdHistory.AddSnapshot(tick, snapshot, key);
        }

        foreach (var kvp in Depletions)
        {
            var losses = kvp.Value;
            foreach (var kvp2 in losses.Contents)
            {
                var deposit = (ResourceDeposit) key.Data[kvp2.Key];
                var loss = kvp2.Value;
                var newSize = Mathf.Max(0f, deposit.Size - loss);
                deposit.Set(nameof(deposit.Size), newSize, key);
            }
        }
    }
}
