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
        
        foreach (var p in _data.Planet.Polygons.Entities)
        {
            if(p.IsLand) PolyPeep.Create(p, key);
        }
        
        foreach (var r in _data.Society.Regimes.Entities)
        {
            GenerateForRegime(r);
        }
        GenerateIndigenous();
        _data.Notices.PopulatedWorld.Invoke();
        report.StopSection("All");

        return report;
    }

    private void GenerateForRegime(Regime r)
    {
        var popSurplus = GenerateFarmsAndFarmers(r);
        var unemployedRatio = .2f;
        var margin = 0f;
        var employed = popSurplus * (1f - (unemployedRatio + margin));
        if (popSurplus <= 0) return;
        var extractionLabor = GenerateExtractionBuildings(r);
        var adminLabor = GenerateTownHalls(r);
        var forFactories = (employed - (extractionLabor + adminLabor));
        GenerateFactories(r, forFactories);
        GenerateLaborers(r, employed);
        GenerateUnemployed(r, Mathf.FloorToInt(popSurplus * unemployedRatio));
    }

    private float GenerateFarmsAndFarmers(Regime r)
    {
        var fertilityPerFarm = _data.GenMultiSettings.SocietySettings.FertilityPerFarm.Value;
        var minFertToGetOneFarm = _data.GenMultiSettings.SocietySettings.FertilityToGetOneFarm.Value;
        var foodConsPerPeep = _data.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        var farmTris = new ConcurrentBag<PolyTriPosition>();
        var farmPolys = new ConcurrentDictionary<MapPolygon, int>();
        var farm = BuildingModelManager.Farm;
        var laborerClass = PeepClassManager.Laborer;
        var territory = r.Polygons.Entities();
        
        Parallel.ForEach(territory, p =>
        {
            if (farm.CanBuildInPoly(p, _data) == false) return;
            var tris = p.Tris.Tris;
            var totalFert = p.Tris.Tris.Count() > 0 
                ? p.Tris.Tris.Select(i => i.GetFertility()).Sum()
                : 0f;

            if (p.GetFertility() * farm.ProductionCap < farm.TotalLaborReq() * foodConsPerPeep) return;
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
            foodSurplus += farm.ProductionCap * farm.GetPolyEfficiencyScore(p.Poly(_data), _data);
        }
        var foodConPerPeep = _data.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        foreach (var kvp in farmPolys)
        {
            var size = farm.TotalLaborReq() * kvp.Value;
            foodSurplus -= foodConPerPeep * size;
            kvp.Key.GetPeep(_data).GrowSize(size, laborerClass, _key);
        }

        return foodSurplus / foodConPerPeep;
    }
    
    private float GenerateExtractionBuildings(Regime r)
    {
        return 0f;
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
            MapBuilding.Create(new PolyTriPosition(p.Id, tri.Index), townHall, _key);
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
                .Where(ind => tris[ind].HasBuilding(_data) == false)
                .OrderBy(ind => tris[ind].GetFertility());
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
        var laborDesire = 0;
        foreach (var p in r.Polygons)
        {
            var buildings = p.GetMapBuildings(_data);
            if (buildings == null) continue;
            
            var laborBuildings = buildings
                .Where(b => b.Model.Model() != BuildingModelManager.Farm)
                .Select(b => b.Model.Model())
                .SelectWhereOfType<BuildingModel, WorkBuildingModel>();
            if (laborBuildings.Count() > 0)
            {
                laborDesire += laborBuildings.Sum(lb => lb.TotalLaborReq());
            }
        }
        var laborRatio = Mathf.Min(1f, popSurplus / laborDesire);
        if (laborRatio == 0) return;
        foreach (var p in r.Polygons)
        {
            var buildings = p.GetMapBuildings(_data);
            if (buildings == null) continue;
            var workBuildings = buildings
                .Where(b => b.Model.Model() != BuildingModelManager.Farm)
                .Select(b => b.Model.Model())
                .SelectWhereOfType<BuildingModel, WorkBuildingModel>();
            var peep = p.GetPeep(_data);
            foreach (var wb in workBuildings)
            {
                foreach (var laborReq in wb.JobLaborReqs)
                {
                    var peepClass = laborReq.Key.PeepClass;
                    var num = Mathf.FloorToInt(laborReq.Value * laborRatio);
                    peep.GrowSize(num, peepClass, _key);
                }
            }
        }
    }

    private void GenerateUnemployed(Regime r, int pop)
    {
        var settlementPolys = r.Polygons.Entities()
            .Where(p => p.HasSettlement(_data))
            .ToList();
        var portions = Apportioner.ApportionLinear(pop, settlementPolys, 
            p => 1);
        for (var i = 0; i < settlementPolys.Count; i++)
        {
            var poly = settlementPolys[i];
            var peep = poly.GetPeep(_data);
            var polyUnemployed = portions[i];
            peep.GrowSize(polyUnemployed, PeepClassManager.Laborer, _key);
        }
    }
    private void GenerateIndigenous()
    {
        var gathererFoodCeiling = _data.BaseDomain.Rules.GathererCeiling;
        var gathererFoodFloor = _data.BaseDomain.Rules.GathererFloor;
        var gathererNeeded = _data.BaseDomain.Rules.GatherLaborCap;
        var foodConsPerPeep = _data.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        var indigenous = PeepClassManager.Indigenous;
        
        foreach (var p in _data.Planet.Polygons.Entities)
        {
            if (p.HasPeep(_data) == false) continue;
            var peep = p.GetPeep(_data);
            var peepSize = peep.Size();
            var foodCapacity = Mathf.Max(0f, p.Moisture - p.Roughness * .25f) * gathererFoodCeiling;
            var peepCapacity = foodCapacity / foodConsPerPeep;

            if (peepCapacity > peepSize)
            {
                var gatherers = Mathf.Min(gathererNeeded, peepCapacity - peepSize);
                peep.GrowSize(Mathf.CeilToInt(gatherers), indigenous, _key);
            }
        }
    }
}