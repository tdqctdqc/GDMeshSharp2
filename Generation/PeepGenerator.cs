using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.ConstraintSolver;

public class PeepGenerator : Generator
{
    private GenData _data;
    private IdDispenser _id;
    private GenWriteKey _key;
    public PeepGenerator()
    {
        
    }

    public override GenReport Generate(GenWriteKey key)
    {
        _key = key;
        _id = key.IdDispenser;
        _data = key.GenData;
        var report = new GenReport(GetType().Name);
        
        report.StartSection();
        foreach (var r in _data.Society.Regimes.Entities)
        {
            GenerateForRegime(r);
        }
        
        _data.Notices.PopulatedWorld.Invoke();
        report.StopSection("All");

        return report;
    }

    private void GenerateForRegime(Regime r)
    {
        var foodSurplus = GenerateFarmersForRegime(r);
        if (foodSurplus <= 0) return;
        GenerateOthersForRegime(r, Mathf.FloorToInt(foodSurplus * .75f));
    }
    private int GenerateFarmersForRegime(Regime r)
    {
        var farmModel = BuildingModelManager.Farm;
        var farmLaborReq = farmModel.JobLaborReqs.Values.Sum();
        var foodConPerPeep = _data.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        var totalFoodProd = 0f;
        var farmerJob = PeepJobManager.Farmer;
        var regimePeepSize = 0;
        foreach (var poly in r.Polygons)
        {
            var polyFarmerSize = 0;
            var buildings = poly.GetMapBuildings(_data);
            if (buildings == null) continue;
            var farms = buildings.Where(b => b.Model.Model() == farmModel);
            var farmCount = farms.Count();
            if (farmCount == 0) continue;
            foreach (var farm in farms)
            {
                var farmProdCap = farmModel.ProductionCap 
                                  * farmModel.GetProductionRatio(farm.Position, 1f, _data);
                var prodPerLabor = farmProdCap / farmLaborReq;
                if (prodPerLabor < foodConPerPeep) continue;
                polyFarmerSize += farmLaborReq;
                totalFoodProd += farmProdCap;
            }
            if (polyFarmerSize == 0)
            {
                continue;
            }
            Peep.Create(
                poly,
                polyFarmerSize,
                _key);
            regimePeepSize += polyFarmerSize;
        }

        return Mathf.FloorToInt(totalFoodProd - regimePeepSize * foodConPerPeep);
    }

    private void GenerateOthersForRegime(Regime r, int foodBudget)
    {
        if (foodBudget <= 0) return;
        var foodConPerPeep = _data.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        var numOthers = foodBudget / foodConPerPeep;
        var polys = r.Polygons.Entities().ToList();
        var portions = Apportioner.ApportionLinear(numOthers, polys, laborDesire);
        for (var i = 0; i < polys.Count; i++)
        {
            var num = Mathf.FloorToInt(portions[i]);
            if (num < 0)
            {
                throw new Exception();
            } 
            if (num == 0) continue;
            Peep.Create(
                polys[i],
                num,
                _key
            );
        }
        
        
        float laborDesire(MapPolygon p)
        {
            var res = 0f;
            var buildings = p.GetMapBuildings(_data);
            if (buildings != null)
            {
                var laborBuildings = buildings.Select(b => b.Model.Model())
                    .SelectWhereOfType<BuildingModel, WorkBuildingModel>();
                if (laborBuildings.Count() > 0)
                {
                    res += laborBuildings.Sum(b => b.JobLaborReqs.Values.Sum());
                }
            }

            if (p.HasSettlement(_data))
            {
                var s = p.GetSettlement(_data);
                foreach (var bm in s.Buildings.Models())
                {
                    if (bm is WorkBuildingModel wm)
                    {
                        res += wm.JobLaborReqs.Values.Sum();
                    }
                }
            }
            return res;
        }
    }
}