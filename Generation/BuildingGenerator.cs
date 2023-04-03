
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class BuildingGenerator : Generator
{
    private GenData _data;
    private GenWriteKey _key;
    public override GenReport Generate(GenWriteKey key)
    {
        _key = key;
        _data = key.GenData;
        var report = new GenReport(nameof(BuildingGenerator));
        
        report.StartSection();
        GenerateFarms();
        report.StopSection(nameof(GenerateFarms));
        
        report.StartSection();
        GenerateMines();
        report.StopSection(nameof(GenerateMines));

        report.StartSection();
        GenerateFactories();
        report.StopSection(nameof(GenerateFactories));
        
        GenerateTownHalls();
        return report;
    }

    private void GenerateFarms()
    {
        var fertilityPerFarm = _data.GenMultiSettings.SocietySettings.FertilityPerFarm.Value;
        var minFertToGetOneFarm = _data.GenMultiSettings.SocietySettings.FertilityToGetOneFarm.Value;
        var farmTris = new ConcurrentBag<PolyTriPosition>();
        var farm = BuildingModelManager.Farm;
        var buildingTris = _data.Society.BuildingAux.ByTri;
        Parallel.ForEach(_data.Planet.Polygons.Entities, p =>
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
            var thisFarmTris = allowedTris
                .OrderByDescending(i => tris[i].GetFertility()).ToList();
            for (var i = 0; i < min; i++)
            {
                var tri = tris[thisFarmTris[i]];
                farmTris.Add(new PolyTriPosition(p.Id, tri.Index));
            }
        });
        
        foreach (var p in farmTris)
        {
            MapBuilding.Create(p, BuildingModelManager.Farm, _key);
        }
    }

    private void GenerateMines()
    {
        var minSizeToGetOneMine = 20f;
        var depositSizePerMine = 100f;
        var mineTris = new ConcurrentDictionary<PolyTriPosition, Item>();
        var mineable = _data.Models.Items.Models.Values.Where(v => v.Attributes.Has<MineableAttribute>());
        
        Parallel.ForEach(_data.Planet.Polygons.Entities, p =>
        {
            if (p.GetResourceDeposits(_data) is IEnumerable<ResourceDeposit> rds == false)
            {
                return;
            }
            var mineableDeposits = rds.Where(rd => mineable.Contains(rd.Item.Model())).ToList();
            if (mineableDeposits.Count() == 0) return;
            var totalSize = mineableDeposits.Sum(d => d.Size);
            var numMines = Mathf.FloorToInt(totalSize / depositSizePerMine);
            var distinctItems = mineableDeposits.Select(d => d.Item.Model()).Distinct().Count();
            if (numMines < distinctItems) numMines = distinctItems;
            var tris = p.Tris.Tris;
            var allowedTris = Enumerable.Range(0, tris.Length)
                .Where(i => tris[i].HasBuilding(_data) == false)
                .Where(i => Mine.CanBuildInTri(tris[i])).ToList();
            if (allowedTris.Count == 0) return;
            if (numMines > allowedTris.Count) numMines = allowedTris.Count;

            var thisMineTris = allowedTris
                .OrderByDescending(i => tris[i].Landform.MinRoughness).ToList();
            for (var i = 0; i < numMines; i++)
            {
                var tri = tris[thisMineTris[i]];
                var item = mineableDeposits.Modulo(i).Item.Model();
                var pos = new PolyTriPosition(p.Id, tri.Index);
                mineTris.TryAdd(pos, item);
            }
        });
        foreach (var p in mineTris)
        {
            var mine = BuildingModelManager.Mines[p.Value];
            MapBuilding.Create(p.Key, mine, _key);
        }
    }

    private void GenerateFactories()
    {
        var settlementSizePerFactory = 20;
        var settlementSizeToGetOneFactory = 10;
        var tris = new ConcurrentBag<PolyTriPosition>();
        var factory = BuildingModelManager.Factory;
        var settlements = _data.Society.Settlements.Entities;

        Parallel.ForEach(settlements, s =>
        {
            if (s.Size < settlementSizeToGetOneFactory) return;
            var poly = s.Poly.Entity();
            var availTris = poly.Tris.Tris.Where(t => factory.CanBuildInTri(t, _data)).ToList();
            if (availTris.Count == 0) return;
            var numFactories = Mathf.CeilToInt((float) s.Size / settlementSizePerFactory);
            numFactories = Mathf.Min(numFactories, availTris.Count);
            for (var i = 0; i < numFactories; i++)
            {
                tris.Add(new PolyTriPosition(poly.Id, availTris[i].Index));
            }
        });
        foreach (var pos in tris)
        {
            MapBuilding.Create(pos, factory, _key);
        }
    }

    private void GenerateTownHalls()
    {
        var townHall = BuildingModelManager.TownHall;
        foreach (var s in _data.Society.Settlements.Entities)
        {
            var p = s.Poly.Entity();
            var tri = p.Tris.Tris.First(t => t.Landform == LandformManager.Urban);
            s.Buildings.AddGen(townHall.Name, _key);
            // MapBuilding.Create(new PolyTriPosition(p.Id, tri.Index), townHall, _key);
        }
    }
}