
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class WorkProdConsumeModule : LogicModule
{
    public override void Calculate(Data data, Action<Message> queueMessage,
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        var proc = WorkProdConsumeProcedure.Create();
        
        foreach (var regime in data.Society.Regimes.Entities)
        {
            var gains = ItemWallet.Construct();
            var depletions = EntityWallet<ResourceDeposit>.Construct();
            proc.RegimeResourceGains.Add(regime.Id, gains);
            proc.Depletions.Add(regime.Id, depletions);
            ProduceForRegime(regime, data, proc);
        }
        
        foreach (var regime in data.Society.Regimes.Entities)
        {
            var consumptions = ItemWallet.Construct();
            proc.ConsumptionsByRegime.Add(regime.Id, consumptions);
            var demands = ItemWallet.Construct();
            proc.DemandsByRegime.Add(regime.Id, demands);
            ConsumeForRegime(proc, regime, data);
        }

        queueMessage(proc);
    }

    private void ProduceForRegime(Regime regime, Data data, WorkProdConsumeProcedure proc)
    {
        var polys = regime.Polygons;
        var er = EmploymentReport.Construct();
        foreach (var poly in polys)
        {
            var buildings = poly.GetBuildings(data);
            if (buildings == null) continue;
            var workBuildings = buildings.Where(b => b.Model.Model() is WorkBuildingModel)
                .Select(b => (WorkBuildingModel)b.Model.Model());
            if (workBuildings.Count() == 0) continue;
            var peeps = poly.GetPeeps(data);
            if (peeps == null) continue;
            var jobNeeds = workBuildings.SelectMany(b => b.JobLaborReqs);
            var jobNeedCount = jobNeeds.Sum(kvp => kvp.Value);
            var jobFilledCount = peeps.Sum(p => p.Size);
            var ratio = (float) jobNeedCount / jobFilledCount;
            ratio = Mathf.Clamp(ratio, 0f, 1f);

            var employment = EmploymentReport.Construct();
            foreach (var kvp in jobNeeds)
            {
                employment.Counts.AddOrSum(kvp.Key.Name, Mathf.CeilToInt((float) kvp.Value * ratio));
            }
            proc.EmploymentReports.Add(poly.Id, employment);
            
            foreach (var building in buildings)
            {
                if (building.Model.Model() is WorkBuildingModel wb)
                {
                    wb.Produce(proc, building, ratio, data);
                }
            }
        }
    }

    private void ConsumeForRegime(WorkProdConsumeProcedure proc, Regime regime, Data data)
    {
        var numPeeps = regime.Polygons
            .Where(p => p.GetPeeps(data) != null)
            .SelectMany(p => p.GetPeeps(data)).Sum(p => p.Size);
        var foodDesired = numPeeps * data.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        proc.DemandsByRegime[regime.Id].Add(ItemManager.Food, foodDesired);
        var foodStock = regime.Items[ItemManager.Food] + proc.RegimeResourceGains[regime.Id][ItemManager.Food];
        var foodConsumption = Mathf.Min(foodDesired, foodStock);
        //todo implement effect
        var foodDeficit = foodConsumption - foodDesired;
        proc.ConsumptionsByRegime[regime.Id].Add(ItemManager.Food, foodConsumption);
    }
}
