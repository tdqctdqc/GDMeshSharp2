using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ConstructionAI : LogicModule
{
    private Regime _regime;
    private static float _proportionOfPolysToBuildIn = .2f;
    private Dictionary<Item, CapacityConstructionQueue> _capacitiesInConstruction;
    public ConstructionAI(Data data, Regime regime)
    {
        _regime = regime;
        _capacitiesInConstruction = new Dictionary<Item, CapacityConstructionQueue>();
    }

    public override void Calculate(Data data, 
        Action<Message> queueMessage, 
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        ConstructFoodProd(data, queueMessage, queueEntityCreation);
    }

    private void ConstructFoodProd(Data data, 
        Action<Message> queueMessage, 
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        var rules = data.BaseDomain.Rules;
        var hist = _regime.History;
        var foodDemand = hist.DemandHistory.GetLatest(ItemManager.Food.Name);
        var desiredFoodProd = foodDemand * rules.MaxEffectiveSurplusRatio;
        ConstructCapacity(ItemManager.Food, desiredFoodProd, data, queueMessage,
            queueEntityCreation);
    }
    private void ConstructCapacity(Item item, 
        float desiredProd,
        Data data, 
        Action<Message> queueMessage, 
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        var hist = _regime.History;
        var prod = hist.ProdHistory.GetLatest(item.Name);
        var capQueue = _capacitiesInConstruction.GetOrAdd(item, i => new CapacityConstructionQueue());
        var currentCons = data.Society.CurrentConstruction;
        
        if (prod + capQueue.CapacityInConstruction <= desiredProd)
        {
            var building = data.Models.Buildings.Models.Values
                .SelectWhereOfType<BuildingModel, ProductionBuildingModel>()
                .Where(pb => pb.ProdItem == item)
                .OrderBy(pb => pb.ProductionCap / pb.JobLaborReqs.Values.Sum())
                .First();
            var laborReq = building.JobLaborReqs.Values.Sum();
            var polysByEfficiency = _regime.Polygons.Entities()
                .Where(p => building.CanBuildInPoly(p, data))
                .OrderByDescending(p => building.GetPolyEfficiencyScore(p, data)).ToList();

            var numToBuild = Mathf.CeilToInt(_regime.Polygons.Count() * _proportionOfPolysToBuildIn);
            var iter = 0;
            for (var i = 0; i < polysByEfficiency.Count; i++)
            {
                var poly = polysByEfficiency[i];
                if (currentCons.ByPoly.ContainsKey(poly.Id)) continue;
                var tris = poly.Tris.Tris
                    .Where(t => building.CanBuildInTri(t, data));
                if(tris.Count() == 0) continue;
                var tri = tris
                    .OrderBy(t => building.GetTriEfficiencyScore(new PolyTriPosition(poly.Id, t.Index), data))
                    .First();
                var proc = StartConstructionProcedure.Construct(
                    building.MakeRef<BuildingModel>(),
                    new PolyTriPosition(poly.Id, tri.Index),
                    _regime.MakeRef()
                );
                capQueue.AddConstruction(proc.Construction, data);
                queueMessage(proc);
                iter++;
                if (iter >= numToBuild) break;
            }
        }
    }
}
