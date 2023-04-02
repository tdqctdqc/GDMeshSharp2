
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class WorkProdConsumeModule : LogicModule
{
    public override void Calculate(Data data, Action<Message> queueMessage,
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        var gainsByRegime = new Dictionary<int, ItemWallet>();
        var demandsByRegime = new Dictionary<int, ItemWallet>();
        var depletionsByRegime = new Dictionary<int, EntityWallet<ResourceDeposit>>();
        var consumptionsByRegime = new Dictionary<int, ItemWallet>();
        var proc = WorkProdConsumeProcedure.Create(gainsByRegime, 
            depletionsByRegime, consumptionsByRegime, demandsByRegime);
        
        foreach (var regime in data.Society.Regimes.Entities)
        {
            var gains = ItemWallet.Construct();
            var depletions = EntityWallet<ResourceDeposit>.Construct();
            gainsByRegime.Add(regime.Id, gains);
            depletionsByRegime.Add(regime.Id, depletions);
            ProduceForRegime(regime, data, proc);
        }
        
        foreach (var regime in data.Society.Regimes.Entities)
        {
            var consumptions = ItemWallet.Construct();
            consumptionsByRegime.Add(regime.Id, consumptions);
            var demands = ItemWallet.Construct();
            demandsByRegime.Add(regime.Id, demands);
            ConsumeForRegime(regime, data, consumptions, demands, gainsByRegime[regime.Id]);
        }

        queueMessage(proc);
    }

    private void ProduceForRegime(Regime regime, Data data, WorkProdConsumeProcedure proc)
    {
        var polys = regime.Polygons;
        foreach (var poly in polys)
        {
            var buildings = poly.GetBuildings(data);
            if (buildings == null) continue;
            var peeps = poly.GetPeeps(data);
            if (peeps == null) continue;
            var jobNeedCounts = buildings.Where(b => b.Model.Model() is WorkBuildingModel)
                .SelectMany(b => ((WorkBuildingModel)b.Model.Model()).JobLaborReqs)
                .SortInto(kvp => kvp.Key, kvp => kvp.Value);
            var jobFilledCounts = peeps.SelectMany(p => p.Jobs.Values)
                .SortInto(ja => ja.Job.Model(), ja => ja.Count);
            
            foreach (var building in buildings)
            {
                if (building.Model.Model() is WorkBuildingModel wb)
                {
                    var staffRatio = 1f;
                    foreach (var job in wb.JobLaborReqs.Keys)
                    {
                        if (jobFilledCounts.ContainsKey(job) == false)
                        {
                            staffRatio = 0f;
                            break;
                        }
                        staffRatio = Mathf.Min(staffRatio, jobFilledCounts[job] / jobNeedCounts[job]);
                    }
                    wb.Produce(proc, building, staffRatio, data);
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
        var foodDesired = numPeeps * data.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        demands.Add(ItemManager.Food, foodDesired);
        var foodStock = regime.Items[ItemManager.Food] + gains[ItemManager.Food];
        var foodConsumption = Mathf.Min(foodDesired, foodStock);
        //todo implement effect
        var foodDeficit = foodConsumption - foodDesired;
        consumptions.Add(ItemManager.Food, foodConsumption);
    }
}
