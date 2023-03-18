
using System;
using System.Linq;
using Godot;

public class PeepConsumptionModule : LogicModule
{
    public override void Calculate(Data data, Action<Message> queue)
    {
        var wallets = RegimeModelWallet<Item>.Construct();
        foreach (var regime in data.Society.Regimes.Entities)
        {
            var foodDesired = regime.Polygons
                .Where(p => p.GetPeeps(data) != null)
                .SelectMany(p => p.GetPeeps(data)).Count();
            var foodStock = regime.Resources[ItemManager.Food];
            if (foodStock == 0f) continue;
            var foodConsumption = Mathf.Min(foodDesired, foodStock);
            
            //todo implement
            var foodDeficit = foodConsumption - foodDesired;
            var wallet = wallets.AddOrGet(regime.MakeRef());
            wallet.Add(ItemManager.Food.MakeRef<Item>(), foodConsumption);
        }
        queue(ConsumptionProcedure.Create(wallets));
    }
}
