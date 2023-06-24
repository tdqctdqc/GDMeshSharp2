using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BudgetAI 
{
    private Regime _regime;
    private Dictionary<Item, BudgetPriority> _priorities;
    public BudgetAI(Data data, Regime regime)
    {
        _regime = regime;
        _priorities = new Dictionary<Item, BudgetPriority>
        {
            {ItemManager.Food, new ProdBuildingConstructBudgetPriority(ItemManager.Food, 
                (r,d) => 1f)},
            {ItemManager.IndustrialPoint, new ProdBuildingConstructBudgetPriority(ItemManager.IndustrialPoint, 
                (r,d) => 1f)},
        };
    }

    public void Calculate(Data data, 
        MajorTurnOrders orders)
    {
        var prices = data.Models.Items.Models.Values.ToDictionary(v => v, v => 1f);
        var totalPrice =
            _regime.Items.Contents.Sum(kvp => prices[(Item) data.Models[kvp.Key]] * _regime.Items[kvp.Key]);
        var totalLaborAvail = _regime.Polygons.Sum(p => p.Employment.NumUnemployed());
        foreach (var kvp in _priorities)
        {
            kvp.Value.SetWeight(data, _regime);
        }
        var totalPriorityWeight = _priorities.Sum(kvp => kvp.Value.Weight);
        var budget = ItemWallet.Construct(_regime.Items);
        foreach (var kvp in _priorities)
        {
            DoPriority(kvp.Value, data, prices, budget, totalPriorityWeight, totalPrice, 
                totalLaborAvail, orders);
        }
    }

    private void DoPriority(BudgetPriority priority, Data data, Dictionary<Item, float> prices, 
        ItemWallet budget, float totalPriorityWeight, float totalPrice, int totalLaborAvail, 
        MajorTurnOrders orders)
    {
        var priorityWeight = priority.Weight;
        var priorityShare = priorityWeight / totalPriorityWeight;
        var credit = Mathf.FloorToInt(totalPrice *  priorityShare);
        var labor = Mathf.FloorToInt(priorityShare * totalLaborAvail);
        if (credit < 0f)
        {
            throw new Exception($"priority weight {priorityWeight} " +
                                $"total weight {totalPriorityWeight} " +
                                $"total price {totalPrice}");
        }
        priority.Calculate(_regime, data, budget, prices, credit,
            labor, orders);
    }
}
