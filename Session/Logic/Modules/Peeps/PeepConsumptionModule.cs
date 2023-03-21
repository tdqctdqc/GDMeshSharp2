
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PeepConsumptionModule : LogicModule
{
    public override void Calculate(Data data, Action<Message> queue)
    {
        var wallets = new Dictionary<int, ItemWallet>();
        foreach (var regime in data.Society.Regimes.Entities)
        {
            var foodDesired = regime.Polygons
                .Where(p => p.GetPeeps(data) != null)
                .SelectMany(p => p.GetPeeps(data)).Count();
            var foodStock = regime.Resources[ItemManager.Food];
            var foodConsumption = Mathf.Min(foodDesired, foodStock);
            //todo implement
            var foodDeficit = foodConsumption - foodDesired;
            var wallet = ItemWallet.Construct();
            wallets.Add(regime.Id, wallet);
            wallet.Add(ItemManager.Food, foodConsumption);
        }
        queue(ConsumptionProcedure.Create(wallets));
    }
}
