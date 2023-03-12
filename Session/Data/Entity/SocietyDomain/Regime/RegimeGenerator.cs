using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeGenerator : Generator
{
    private GenData _data;
    private IdDispenser _id;
    private GenWriteKey _key;
    public RegimeGenerator()
    {
        
    }

    public override GenReport Generate(GenWriteKey key)
    {
        _id = key.IdDispenser;
        _key = key;
        _data = key.GenData;
        var report = new GenReport(GetType().Name);
        report.StartSection();
        GenerateRegimes();
        report.StopSection("all");
        return report;
    }

    private void GenerateRegimes()
    {
        var polysPerRegime = 30;
        
        
        _data.LandSea.Landmasses.ForEach(lm =>
        {
            GenerateRegimes(lm, polysPerRegime);
        });
    }

    // private (List<Regime> regimes, Picker picker) GenerateLandmassRegimes(HashSet<MapPolygon> lm, int polysPerRegime)
    // {
    //     
    // }
    private void GenerateRegimes(HashSet<MapPolygon> lm, int polysPerRegime)
    {
        var numLandmassRegimes = Mathf.CeilToInt(lm.Count / polysPerRegime);
        numLandmassRegimes = Math.Max(1, numLandmassRegimes);
        var seeds = lm.GetDistinctRandomElements(numLandmassRegimes);
        var picker = new WandererPicker(lm);


        for (var i = 0; i < seeds.Count; i++)
        {
            var prim = ColorsExt.GetRandomColor();
            var sec = prim.Inverted();
            var regime = Regime.Create(_id, NameGenerator.GetName(), prim, sec, seeds[i], _key);
            var wand = new RegimeWanderer(regime, seeds[i], picker);
            seeds[i].SetRegime(regime, _key);
        }
        
        
        var wanderers = new List<RegimeWanderer>();
        picker.Pick();
        
        foreach (var w in picker.Wanderers)
        {
            var r = ((RegimeWanderer) w).Regime;
            foreach (var p in w.Picked)
            {
                r.Polygons.AddRef(p, _key);
                p.SetRegime(r, _key);
            }
        }

        var remainder = picker.NotTaken;
        var unions = UnionFind.Find(
            remainder, 
            (p1, p2) => p1.IsLand() == p2.IsLand(),
            p => p.Neighbors.Refs()
        );
        foreach (var union in unions)
        {
            if (union.Count == 0) continue;
            var prim = ColorsExt.GetRandomColor();
            var sec = prim.Inverted();
            var regime = Regime.Create(_id, NameGenerator.GetName(), prim, sec, union[0], _key);
            for (var i = 1; i < union.Count; i++)
            {
                var p = union[i];
                regime.Polygons.AddRef(p, _key);
                p.SetRegime(regime, _key);
            }
        }
    }

    private void ExpandRegimes()
    {
        
    }
}