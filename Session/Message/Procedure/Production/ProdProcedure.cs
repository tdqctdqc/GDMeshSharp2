
using System.Linq;
using System.Collections.Generic;
using Godot;
using MessagePack;

public class ProdProcedure : Procedure
{
    public RegimeModelWallet<Item> RegimeResourceGains { get; private set; }
    public RegimeEntityWallet<ResourceDeposit> Depletions { get; private set; }

    public static ProdProcedure Create(RegimeModelWallet<Item> resourceGains, 
        RegimeEntityWallet<ResourceDeposit> depletions)
    {
        return new ProdProcedure(resourceGains, depletions);
    }
    [SerializationConstructor] private ProdProcedure(
        RegimeModelWallet<Item> regimeResourceGains, 
        RegimeEntityWallet<ResourceDeposit> depletions)
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
        foreach (var kvp in RegimeResourceGains.Wallets)
        {
            var r = kvp.Key.Entity();
            var gains = kvp.Value;
            foreach (var kvp2 in gains.Contents)
            {
                r.Resources.Add(kvp2.Key.Model(), kvp2.Value);
            }
        }

        foreach (var kvp in Depletions.Wallets)
        {
            var regime = kvp.Key.Entity();
            var losses = kvp.Value;
            foreach (var kvp2 in losses.Contents)
            {
                var deposit = kvp2.Key.Entity();
                var loss = kvp2.Value;
                var newSize = Mathf.Max(0f, deposit.Size - loss);
                deposit.Set(nameof(deposit.Size), newSize, key);
            }
        }
    }
}
