
using System;
using System.Collections.Concurrent;
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
        
        
        return report;
    }

    private void GenerateFarms()
    {
        var fertilityPerFarm = 15_000f;
        var minFertToGetOneFarm = 7_500f;
        var farmTris = new ConcurrentBag<PolyTriPosition>();
        var farm = BuildingModelManager.Farm;
        Parallel.ForEach(_data.Planet.Polygons.Entities, p =>
        {
            if (farm.CanBuildInPoly(p, _data) == false) return;
            var tris = p.TerrainTris.Tris;
            var fert = p.GetFertility();
            var numFarms = Mathf.FloorToInt(fert / fertilityPerFarm);
            if (numFarms == 0 && fert > minFertToGetOneFarm) numFarms = 1;
            if (numFarms == 0) return;
            var allowedTris = Enumerable.Range(0, tris.Length)
                .Where(i => farm.CanBuildInTri(tris[i], _data)).ToList();
            if (allowedTris.Count == 0) return;
            var min = Math.Min(numFarms, allowedTris.Count);
            var thisFarmTris = allowedTris
                .OrderByDescending(i => tris[i].GetFertility()).ToList();
            for (var i = 0; i < min; i++)
            {
                var tri = thisFarmTris[i];
                farmTris.Add(new PolyTriPosition(p.MakeRef(), tri));
            }
        });
        
        foreach (var p in farmTris)
        {
            Building.Create(p, BuildingModelManager.Farm, _key);
        }
    }
}