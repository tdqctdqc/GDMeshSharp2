
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ProductionAndConsumptionModule : LogicModule
{
    public override void Calculate(Data data, Action<Message> queue)
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
        queue(proc);
    }

    private void ProduceForRegime(Regime regime, Data data, ItemWallet gains,
        EntityWallet<ResourceDeposit> depletions)
    {
        var polys = regime.Polygons;
        var avail = new HashSet<Peep>();
        foreach (var poly in polys)
        {
            var buildings = poly.GetBuildings(data);
            if (buildings == null) continue;
            var peeps = poly.GetPeeps(data);
            if (peeps == null) continue;
            var notTakenPeeps = peeps.ToHashSet();
            foreach (var building in buildings)
            {
                if (notTakenPeeps.Count == 0) break;
                if (building.Model.Model() is ProductionBuilding rb)
                {
                    avail.Clear();
                    avail.AddRange(notTakenPeeps.Where(p => rb.JobTypes.Contains(p.Job.Model())));
                    if (avail.Count == 0) continue;
                    var num = Mathf.Min(rb.PeepsLaborReq, avail.Count());
                    if (num == 0) continue;
                    for (var i = 0; i < num; i++)
                    {
                        notTakenPeeps.Remove(avail.ElementAt(i));
                    }
                    var staffRatio = (float) num / rb.PeepsLaborReq;
                    rb.Produce(gains, depletions, building, staffRatio, data);
                }
            }
        }
    }

    private void ConsumeForRegime(Regime regime, Data data, ItemWallet consumptions, ItemWallet demands,
        ItemWallet gains)
    {
        var numPeeps = regime.Polygons
            .Where(p => p.GetPeeps(data) != null)
            .SelectMany(p => p.GetPeeps(data)).Count();
        var foodDesired = numPeeps * data.BaseDomain.RuleVars.Value.FoodConsumptionPerPeep;
        demands.Add(ItemManager.Food, foodDesired);
        var foodStock = regime.Items[ItemManager.Food] + gains[ItemManager.Food];
        var foodConsumption = Mathf.Min(foodDesired, foodStock);
        //todo implement effect
        var foodDeficit = foodConsumption - foodDesired;
        consumptions.Add(ItemManager.Food, foodConsumption);
    }
}
