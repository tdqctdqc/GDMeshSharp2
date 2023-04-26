using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        var popSurplus = GenerateFarmsAndFarmers(r);
        if (popSurplus <= 0) return;
        var extractionLabor = GenerateExtractionBuildings(r);
        var adminLabor = GenerateTownHalls(r);
        var forFactories = (popSurplus - (extractionLabor + adminLabor)) * .75f;
        GenerateFactories(r, forFactories);
        GenerateLaborers(r, popSurplus);
    }

    private float GenerateFarmsAndFarmers(Regime r)
    {
        var fertilityPerFarm = _data.GenMultiSettings.SocietySettings.FertilityPerFarm.Value;
        var minFertToGetOneFarm = _data.GenMultiSettings.SocietySettings.FertilityToGetOneFarm.Value;
        var farmTris = new ConcurrentBag<PolyTriPosition>();
        var farmPolys = new ConcurrentDictionary<MapPolygon, int>();
        var farm = BuildingModelManager.Farm;
        var buildingTris = _data.Society.BuildingAux.ByTri;
        var territory = r.Polygons.Entities();
        
        Parallel.ForEach(territory, p =>
        {
            if (farm.CanBuildInPoly(p, _data) == false) return;
            var tris = p.Tris.Tris;
            var totalFert = p.GetFertility();
            var numFarms = Mathf.FloorToInt(totalFert / fertilityPerFarm);
            if (numFarms == 0 && totalFert > minFertToGetOneFarm) numFarms = 1;
            if (numFarms == 0) return;
            var allowedTris = Enumerable.Range(0, tris.Length)
                .Where(i => tris[i].HasBuilding(_data) == false)
                .Where(i => farm.CanBuildInTri(tris[i], _data));
            if (allowedTris.Count() == 0) return;
            var min = Math.Min(numFarms, allowedTris.Count());
            farmPolys.GetOrAdd(p, min);
            var thisFarmTris = allowedTris
                .OrderByDescending(i => tris[i].GetFertility()).ToList();
            for (var i = 0; i < min; i++)
            {
                var tri = tris[thisFarmTris[i]];
                farmTris.Add(new PolyTriPosition(p.Id, tri.Index));
            }
        });
        float foodSurplus = 0f;
        foreach (var p in farmTris)
        {
            MapBuilding.Create(p, BuildingModelManager.Farm, _key);
            foodSurplus += farm.ProductionCap * farm.GetTriEfficiencyScore(p, _data);
        }
        var foodConPerPeep = _data.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        foreach (var kvp in farmPolys)
        {
            var size = farm.TotalLaborReq() * kvp.Value;
            foodSurplus -= foodConPerPeep * size;
            var peep = Peep.Create(kvp.Key, size, _key);
        }

        return foodSurplus / foodConPerPeep;
    }
    
    private float GenerateExtractionBuildings(Regime r)
    {
        var extractBuildings = _data.Models.Buildings.Models
            .Values.SelectWhereOfType<BuildingModel, ExtractionBuildingModel>()
            .ToDictionary(m => m.ProdItem, m => m);
        
        var triPoses = new List<PolyTriPosition>();
        var buildings = new List<ExtractionBuildingModel>();
        
        foreach (var p in r.Polygons)
        {
            if (p.GetResourceDeposits(_data) is IEnumerable<ResourceDeposit> rds == false)
            {
                continue;
            }
            var tris = p.Tris.Tris;
            var avail = tris.Select((t,i) => i)
                .Where(i => tris[i].HasBuilding(_data) == false);
            var ll = new LinkedList<int>(avail);
            foreach (var rd in rds)
            {
                if (extractBuildings.ContainsKey(rd.Item.Model()) == false) continue;
                var b = extractBuildings[rd.Item.Model()];
                var triIndex = ll.First(i => b.CanBuildInTri(tris[i], _data));
                ll.Remove(triIndex);
                triPoses.Add(new PolyTriPosition(p.Id, (byte)triIndex));
                buildings.Add(b);
            }
        }

        var laborDemand = 0f;
        for (var i = 0; i < triPoses.Count; i++)
        {
            var pos = triPoses[i];
            var b = buildings[i];
            laborDemand += b.JobLaborReqs.Sum(kvp => kvp.Value);
            MapBuilding.Create(pos, b, _key);
        }

        return laborDemand;
    }
    private float GenerateTownHalls(Regime r)
    {
        var townHall = BuildingModelManager.TownHall;
        var settlements = r.Polygons.Where(p => p.HasSettlement(_data))
            .Select(p => p.GetSettlement(_data));
        foreach (var s in settlements)
        {
            var p = s.Poly.Entity();
            var tri = p.Tris.Tris.First(t => t.Landform == LandformManager.Urban);
            s.Buildings.AddGen(townHall.Name, _key);
        }

        return townHall.TotalLaborReq() * settlements.Count();
    }
    private void GenerateFactories(Regime r, float popBudget)
    {
        if (popBudget <= 0) return;
        var polys = r.Polygons.Entities().ToList();
        var portions = Apportioner.ApportionLinear(popBudget, polys,
            p =>
            {
                return Mathf.Max(0f, p.Moisture - p.Roughness);
                // var ps = p.GetPeeps(_data);
                // if (ps == null) return 0f;
                // return p.GetPeeps(_data).Sum(x => x.Size);
            }
        );
        var factory = BuildingModelManager.Factory;
        var factoryLaborReq = factory.TotalLaborReq();
        var factoryTris = new List<PolyTriPosition>();
        for (var i = 0; i < polys.Count; i++)
        {
            var p = polys[i];
            var pop = portions[i];
            var numFactories = Mathf.Round(pop / factoryLaborReq);
            var tris = p.Tris.Tris;
            var avail = tris.Select((t,ind) => ind)
                .Where(ind => tris[ind].HasBuilding(_data) == false);
            if (avail.Count() < numFactories) numFactories = avail.Count();
            
            for (var j = 0; j < numFactories; j++)
            {
                var triIndex = avail.ElementAt(j);
                factoryTris.Add( new PolyTriPosition(p.Id, (byte)triIndex));
            }
        }
        foreach (var pt in factoryTris)
        {
            MapBuilding.Create(pt, factory, _key);
        }
    }
    private void GenerateLaborers(Regime r, float popSurplus)
    {
        if (popSurplus <= 0) return;
        var polys = r.Polygons.Entities().ToList();
        var portions = Apportioner.ApportionLinear(popSurplus, polys, 
            p => Mathf.Max(0f, laborDesire(p) + (p.Moisture - p.Roughness) * 100f));
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
                    res += laborBuildings.Sum(b => b.TotalLaborReq());
                }
            }

            if (p.HasSettlement(_data))
            {
                var s = p.GetSettlement(_data);
                foreach (var bm in s.Buildings.Models())
                {
                    if (bm is WorkBuildingModel wm)
                    {
                        res += wm.TotalLaborReq();
                    }
                }
            }
            return res;
        }
    }
}