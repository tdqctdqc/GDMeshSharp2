using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeGenerator
{
    private GenData _data;
    private IdDispenser _id;
    private GenWriteKey _key;
    public RegimeGenerator(GenData data, IdDispenser id, GenWriteKey key)
    {
        _id = id;
        _key = key;
        _data = data;
    }

    public void Generate()
    {
        GenerateRegimes();
    }

    private void GenerateRegimes()
    {
        var polysPerRegime = 30;

        _data.LandSea.Landmasses.ForEach(lm =>
        {
            GenerateLandmassRegimes(lm, polysPerRegime);
        });
    }

    private void GenerateLandmassRegimes(HashSet<MapPolygon> lm, int polysPerRegime)
    {
        var landmassRegimes = Mathf.CeilToInt(lm.Count / polysPerRegime);
        landmassRegimes = Math.Max(1, landmassRegimes);
        var seeds = lm.GetNRandomElements(landmassRegimes);
        var wanderers = new List<RegimeWanderer>();
        var picker = new WandererPicker(lm);

        for (var i = 0; i < seeds.Count; i++)
        {
            var prim = ColorsExt.GetRandomColor();
            var sec = prim.Inverted();
            var regime = Regime.Create(_id, NameGenerator.GetName(), prim, sec, seeds[i], _key);
            var wand = new RegimeWanderer(regime, seeds[i], picker);
            seeds[i].SetRegime(regime, _key);
        }
        
        picker.Pick();
        
        foreach (var w in picker.Wanderers)
        {
            var r = ((RegimeWanderer) w).Regime;
            foreach (var p in w.Picked)
            {
                r.Polygons.AddRef(p, _key.Data);
                p.SetRegime(r, _key);
            }
        }

        var remainder = picker.NotTaken;
        var unions = UnionFind<MapPolygon>.DoUnionFind(
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
                regime.Polygons.AddRef(p, _key.Data);
                p.SetRegime(regime, _key);
            }
        }
    }
}