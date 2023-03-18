
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ProductionModule : LogicModule
{
    public override void Calculate(Data data, Action<Message> queue)
    {
        var regimeResourceGains = RegimeModelWallet<Item>.Construct();
        var regimeDepletions = RegimeEntityWallet<ResourceDeposit>.Construct();
        var avail = new List<Peep>();
        foreach (var regime in data.Society.Regimes.Entities)
        {
            var polys = regime.Polygons;
            var resourceGains = regimeResourceGains.AddOrGet(regime.MakeRef());
            var depletions = regimeDepletions.AddOrGet(regime.MakeRef());
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
                        rb.Produce(resourceGains, depletions, building, staffRatio, data);
                    }
                }
            }

            queue(ProdProcedure.Create(regimeResourceGains, regimeDepletions));
        }
        
    }
}
