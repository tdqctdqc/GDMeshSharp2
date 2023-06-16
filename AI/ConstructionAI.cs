using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ConstructionAI 
{
    private Regime _regime;
    public ConstructionAI(Data data, Regime regime)
    {
        _regime = regime;
    }

    public void Calculate(Data data, 
        Action<Message> queueMessage, 
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        var prices = data.Models.Items.Models.Values.ToDictionary(v => v, v => 1f);
        var credit = int.MaxValue;
        //prices.Sum(kvp => kvp.Value * _regime.Items[kvp.Key]);
        // var budget = ItemWallet.Construct();
        new BuildingConstructAiPriority(ItemManager.Food, 1f)
            .Calculate(_regime, data, _regime.Items, prices, credit,
                queueMessage, queueEntityCreation);
        new BuildingConstructAiPriority(ItemManager.IndustrialPoint, 1f)
            .Calculate(_regime, data, _regime.Items, prices, credit,
                queueMessage, queueEntityCreation);
    }
}
