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
        
        // report.StartSection();
        // var byPoly = _data.Society.Settlements.ByPoly;
        // GeneratePeepType(PeepJobManager.Laborer,
        //     p => (int)(byPoly.ContainsKey(p)
        //         ? byPoly[p].Size * 100
        //         : 0), 
        //     50, 50);
        report.StopSection("All");

        return report;
    }

    private void GenerateForRegime(Regime r)
    {
        var foodSurplus = GenerateFarmersForRegime(r);
        GenerateLaborersForRegime(r, Mathf.FloorToInt(foodSurplus * .25f));
    }
    private int GenerateFarmersForRegime(Regime r)
    {
        var farmModel = BuildingModelManager.Farm;
        var farmLaborReq = farmModel.PeepsLaborReq;
        var foodConPerPeep = _data.BaseDomain.RuleVars.Value.FoodConsumptionPerPeep;
        var totalFoodProd = 0f;
        var totalNumFarmers = 0;
        var farmerJob = PeepJobManager.Farmer;
        foreach (var poly in r.Polygons)
        {
            var buildings = poly.GetBuildings(_data);
            if (buildings == null) continue;
            var farms = buildings.Where(b => b.Model.Model() == farmModel);
            var farmCount = farms.Count();
            if (farmCount == 0) continue;
            //todo foodProd not taking into account fertility
            foreach (var farm in farms)
            {
                var farmProdCap = farmModel.FullProduction * farm.Position.Tri().GetFertility();
                var prodPerPeep = farmProdCap / farmLaborReq;
                if (prodPerPeep < foodConPerPeep) continue;
                var surplusPerFarmer = prodPerPeep - foodConPerPeep;
                var surplusRatio = surplusPerFarmer / prodPerPeep;
                var numFarmers = Mathf.CeilToInt(Mathf.Sqrt(surplusRatio) * farmLaborReq);
                numFarmers = Mathf.Clamp(numFarmers, farmCount, farmCount * farmLaborReq);
                totalNumFarmers += numFarmers;
                totalFoodProd += ((float)numFarmers / farmLaborReq) * farmProdCap;
                for (var i = 0; i < numFarmers; i++)
                {
                    Peep.Create(
                        poly,
                        farmerJob,
                        _key);
                }
            }
        }

        return Mathf.FloorToInt(totalFoodProd - totalNumFarmers * foodConPerPeep);
    }

    private void GenerateLaborersForRegime(Regime r, int foodBudget)
    {
        var foodConPerPeep = _data.BaseDomain.RuleVars.Value.FoodConsumptionPerPeep;
        var laborerJob = PeepJobManager.Laborer;
        var numLaborers = (foodBudget / foodConPerPeep);
        var polys = r.Polygons.Entities().ToList();
        var portions = Apportioner.ApportionLinear(numLaborers, polys, laborDesire);
        for (var i = 0; i < polys.Count; i++)
        {
            var num = Mathf.FloorToInt(portions[i]);
            for (var j = 0; j < num; j++)
            {
                Peep.Create(
                    polys[i],
                    laborerJob,
                    _key
                );
            }
        }
        
        
        float laborDesire(MapPolygon p)
        {
            var res = 0f;
            var buildings = p.GetBuildings(_data);
            if (buildings != null)
            {
                var laborBuildings = buildings.Select(b => b.Model.Model())
                    .SelectWhereOfType<BuildingModel, ProductionBuilding>()
                    .Where(b => b.JobTypes.Contains(laborerJob));
                if (laborBuildings.Count() > 0)
                {
                    res += laborBuildings.Sum(b => b.PeepsLaborReq);
                }
            }

            res += p.TerrainTris.Tris.Where(t => t.Landform == LandformManager.Urban).Count() * 5;
            return res;
        }
    }
    private void GeneratePeepType(PeepJob job, Func<MapPolygon, int> getPoints,
        int minPoints, int pointsPerPeep)
    {
        var numPeeps = 0;
        var totalPoints = 0;
        foreach (var poly in _data.Planet.Polygons.Entities)
        {
            var pointsInPoly = getPoints(poly);
            totalPoints += pointsInPoly;
            if (pointsInPoly < minPoints)
            {
                continue;
            }
            
            var polyPeeps = Mathf.CeilToInt(pointsInPoly / pointsPerPeep);

            numPeeps += polyPeeps;
            for (int i = 0; i < polyPeeps; i++)
            {
                var peep = Peep.Create(
                    poly,
                    job,
                    _key);
            }
        }
    }
}