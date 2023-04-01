
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
            
            foreach (var building in buildings)
            {
                if (building.Model.Model() is ProductionBuildingModel rb)
                {

                    var laborPower = building.NumWorkers(data);
                    if (laborPower == 0) continue;
                    var num = Mathf.Min(rb.PeepsLaborReq, laborPower);
                    if (num == 0) continue;
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
