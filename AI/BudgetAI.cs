using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BudgetAI 
{
    private Regime _regime;
    private Dictionary<Item, AiPriority> _priorities;
    public BudgetAI(Data data, Regime regime)
    {
        _regime = regime;
        _priorities = new Dictionary<Item, AiPriority>
        {
            {ItemManager.Food, new ProdBuildingConstructAiPriority(ItemManager.Food, 1f)},
            {ItemManager.IndustrialPoint, new ProdBuildingConstructAiPriority(ItemManager.IndustrialPoint, 1f)},
        };
    }

    public void Calculate(Data data, 
        Action<Message> queueMessage, 
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        var prices = data.Models.Items.Models.Values.ToDictionary(v => v, v => 1f);
        var totalPrice =
            _regime.Items.Contents.Sum(kvp => prices[(Item) data.Models[kvp.Key]] * _regime.Items[kvp.Key]);
        var totalPriorityWeight = _priorities.Sum(kvp => kvp.Value.GetPriorityWeight(_regime, data));
        var budget = ItemWallet.Construct(_regime.Items);
        foreach (var kvp in _priorities)
        {
            DoPriority(kvp.Key, data, prices, budget, totalPriorityWeight, totalPrice, 
                queueMessage, queueEntityCreation);
        }
    }

    private void DoPriority(Item item, Data data, Dictionary<Item, float> prices, 
        ItemWallet budget,
        float totalPriorityWeight, float totalPrice,
        Action<Message> queueMessage, Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        var priority = _priorities[item];
        var priorityWeight = priority.GetPriorityWeight(_regime, data);
        var credit = Mathf.FloorToInt(totalPrice *  priorityWeight / totalPriorityWeight);
        
        if (credit < 0f)
        {
            throw new Exception($"priority weight {priorityWeight} " +
                                $"total weight {totalPriorityWeight} " +
                                $"total price {totalPrice}");
        }
        new ProdBuildingConstructAiPriority(item, 1f)
            .Calculate(_regime, data, budget, prices, credit,
                queueMessage, queueEntityCreation);
    }
}
