using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ConstructionAI : LogicModule
{
    private Regime _regime;
    private static float _proportionOfPolysToBuildIn = .2f;
    private static float _proportionAvailLaborToBuild = .1f;
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
        var polyConstructionLabor = new Dictionary<int, int>();
        ConstructFoodProd(polyConstructionLabor, data, queueMessage, queueEntityCreation);
        ConstructIndustrialPointsProd(polyConstructionLabor, data, queueMessage,
            queueEntityCreation);
    }

    private void ConstructFoodProd(Dictionary<int, int> polyConstructionLabor, Data data, 
        Action<Message> queueMessage, 
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        var rules = data.BaseDomain.Rules;
        var hist = _regime.History;
        var foodDemand = hist.DemandHistory[ItemManager.Food.Name].GetLatest();
        var desiredFoodProd = foodDemand * rules.MaxEffectiveSurplusRatio;
        ConstructCapacity(ItemManager.Food, desiredFoodProd, polyConstructionLabor,
            data, queueMessage,
            queueEntityCreation);
    }
    
    private void ConstructIndustrialPointsProd(Dictionary<int, int> polyConstructionLabor, Data data, 
        Action<Message> queueMessage, 
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        var rules = data.BaseDomain.Rules;
        var hist = _regime.History;
        var ipDemand = hist.DemandHistory[ItemManager.IndustrialPoint.Name].GetLatest();
        var ipProd = hist.ProdHistory[ItemManager.IndustrialPoint.Name].GetLatest();
        var unemploymentRate = (float)hist.PeepHistory.Unemployed.GetLatest() / hist.PeepHistory.PeepSize.GetLatest();
        
        if (ipDemand > ipProd || unemploymentRate > .05f)
        {
            ConstructCapacity(ItemManager.IndustrialPoint, 
                Mathf.Inf, 
                polyConstructionLabor,
                data, queueMessage,
                queueEntityCreation);
        }
    }
    
    
    private void ConstructCapacity(Item item, 
        float desiredProd,
        Dictionary<int, int> polyConstructionLabor,
        Data data, 
        Action<Message> queueMessage, 
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        // var hist = _regime.History;
        // var prod = hist.ProdHistory[item.Name].GetLatest();
        // var capQueue = _capacitiesInConstruction.GetOrAdd(item, i => new CapacityConstructionQueue());
        // var currentCons = data.Society.CurrentConstruction;
        // if (prod + capQueue.CapacityInConstruction >= desiredProd) return;
        // var building = data.Models.Buildings.Models.Values
        //     .SelectWhereOfType<BuildingModel, ProductionBuildingModel>()
        //     .Where(pb => pb.ProdItem == item)
        //     .OrderBy(pb => pb.ProductionCap / pb.JobLaborReqs.Values.Sum())
        //     .First();
        // var laborReq = building.JobLaborReqs.Values.Sum();
        // var polysByEfficiency = _regime.Polygons.Entities()
        //     .Where(p => building.CanBuildInPoly(p, data))
        //     .OrderByDescending(p => building.GetPolyEfficiencyScore(p, data)).ToList();
        //
        // var numToBuild = Mathf.CeilToInt(_regime.Polygons.Count() * _proportionOfPolysToBuildIn);
        // var iter = 0;
        // for (var i = 0; i < polysByEfficiency.Count; i++)
        // {
        //     var poly = polysByEfficiency[i];
        //     if (currentCons.ByPoly.ContainsKey(poly.Id)) continue;
        //     var tris = poly.Tris.Tris
        //         .Where(t => building.CanBuildInTri(t, data));
        //     if(tris.Count() == 0) continue;
        //     
        //     float unemployed = poly.Employment.NumUnemployed();
        //     if (polyConstructionLabor.ContainsKey(poly.Id))
        //     {
        //         unemployed -= polyConstructionLabor[poly.Id];
        //     }
        //     if (unemployed < building.LaborPerTickToBuild * _proportionAvailLaborToBuild) return;
        //     
        //     polyConstructionLabor.AddOrSum(poly.Id, building.LaborPerTickToBuild);
        //     
        //     var tri = tris
        //         .OrderBy(t => building.GetTriEfficiencyScore(new PolyTriPosition(poly.Id, t.Index), data))
        //         .First();
        //     var proc = StartConstructionProcedure.Construct(
        //         building.MakeRef<BuildingModel>(),
        //         new PolyTriPosition(poly.Id, tri.Index),
        //         _regime.MakeRef()
        //     );
        //
        //     capQueue.AddConstruction(proc.Construction, data);
        //     queueMessage(proc);
        //     iter++;
        //     if (iter >= numToBuild) break;
        // }
    }
}
