
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ProductionAndConsumptionModule : LogicModule
{
    public override void Calculate(Data data, Action<Message> queueMessage,
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        var gainsByRegime = new Dictionary<int, ItemWallet>();
        var demandsByRegime = new Dictionary<int, ItemWallet>();
        var depletionsByRegime = new Dictionary<int, EntityWallet<ResourceDeposit>>();
        var consumptionsByRegime = new Dictionary<int, ItemWallet>();
        
        foreach (var regime in data.Society.Regimes.Entities)
        {
            var gains = ItemWallet.Construct();
            var depletions = EntityWallet<ResourceDeposit>.Construct();
            gainsByRegime.Add(regime.Id, gains);
            depletionsByRegime.Add(regime.Id, depletions);
            ProduceForRegime(regime, data, gains, depletions);
        }
        
        foreach (var regime in data.Society.Regimes.Entities)
        {
            var consumptions = ItemWallet.Construct();
            consumptionsByRegime.Add(regime.Id, consumptions);
            var demands = ItemWallet.Construct();
            demandsByRegime.Add(regime.Id, demands);
            ConsumeForRegime(regime, data, consumptions, demands, gainsByRegime[regime.Id]);
        }

        var proc = ProdAndConsumeProcedure.Create(gainsByRegime, 
            depletionsByRegime, consumptionsByRegime, demandsByRegime);
        queueMessage(proc);
    }

    private void ProduceForRegime(Regime regime, Data data, ItemWallet gains,
        EntityWallet<ResourceDeposit> depletions)
    {
        var polys = regime.Polygons;
        foreach (var poly in polys)
        {
            var buildings = poly.GetBuildings(data);
            if (buildings == null) continue;
            var peeps = poly.GetPeeps(data);
            if (peeps == null) continue;
            var peepAvailCounts = peeps.Sort(p => p.Job, p => p.Size);
            
            foreach (var building in buildings)
            {
                if (building.Model.Model() is ProductionBuilding rb)
                {
                    var avail = peepAvailCounts
                        .Where(kvp => rb.JobTypes.Contains(kvp.Key.Model()));
                    if (avail.Count() == 0) continue;
                    var availNum = avail.Sum(kvp => kvp.Value);
                    if (availNum == 0) continue;
                    var num = Mathf.Min(rb.PeepsLaborReq, availNum);
                    if (num == 0) continue;

                    var toTakeAway = num;
                    
                    
                    var staffRatio = (float) num / rb.PeepsLaborReq;
                    rb.Produce(gains, depletions, building, staffRatio, data);
                    int iter = 0;
                    while (num >= 0)
                    {
                        if (toTakeAway <= 0) break;
                        var kvp = avail.ElementAt(iter);
                        iter++;
                        var takingAway = Mathf.Min(toTakeAway, kvp.Value);
                        toTakeAway -= takingAway;
                        peepAvailCounts[kvp.Key] -= takingAway;
                    }
                }
            }
        }
    }

    private void ConsumeForRegime(Regime regime, Data data, ItemWallet consumptions, ItemWallet demands,
        ItemWallet gains)
    {
        var numPeeps = regime.Polygons
            .Where(p => p.GetPeeps(data) != null)
            .SelectMany(p => p.GetPeeps(data)).Sum(p => p.Size);
        var foodDesired = numPeeps * data.BaseDomain.Rules.FoodConsumptionPerPeep;
        demands.Add(ItemManager.Food, foodDesired);
        var foodStock = regime.Items[ItemManager.Food] + gains[ItemManager.Food];
        var foodConsumption = Mathf.Min(foodDesired, foodStock);
        //todo implement effect
        var foodDeficit = foodConsumption - foodDesired;
        consumptions.Add(ItemManager.Food, foodConsumption);
    }
}
