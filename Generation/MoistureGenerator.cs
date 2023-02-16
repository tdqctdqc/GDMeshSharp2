using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MoistureGenerator
{
    public GenData Data { get; private set; }
    private GenWriteKey _key;
    private IDDispenser _id;
    public MoistureGenerator(GenData data, IDDispenser id)
    {
        Data = data;
        _id = id;
    }

    public void Generate(GenWriteKey key)
    {
        _key = key;
        SetPolyMoistures();
        BuildPolyVegetationTris();
        BuildRivers();
    }
    private void SetPolyMoistures()
    {
        var plateMoistures = new Dictionary<GenPlate, float>();
        
        Data.GenAuxData.Plates.ForEach(p =>
        {
            var distFromEquator = Mathf.Abs(Data.Planet.Height / 2f - p.Center.y);
            var altMult = .5f + .5f * (1f - distFromEquator / (Data.Planet.Height / 2f));
            var polyGeos = p.Cells.SelectMany(c => c.PolyGeos).ToList();
            var count = polyGeos.Count;
            var waterCount = polyGeos.Where(g => g.IsWater()).Count();
            var score = altMult * waterCount / count;
            plateMoistures.Add(p, score);
        });


        float maxFriction = 0f;
        float averageFriction = 0f;
        int iter = 0;
        for (int i = 0; i < 3; i++)
        {
            diffuse();
        }
        GD.Print("max friction " + maxFriction);
        GD.Print("average friction " + (averageFriction / iter));
        
        void diffuse()
        {
            Data.GenAuxData.Plates.ForEach(p =>
            {
                var oldScore = plateMoistures[p];
                
                var newScore = p.Neighbors.Select(n =>
                {
                    var mult = 1f;
                    if (Data.GenAuxData.FaultLines.TryGetFault(p, n, out var fault))
                    {
                        mult = 1f - fault.Friction * .5f;
                        maxFriction = Mathf.Max(maxFriction, fault.Friction);
                        averageFriction += fault.Friction;
                        iter++;
                    }
                    return mult * plateMoistures[n];
                }).Average();

                if (newScore > oldScore)
                {
                    plateMoistures[p] = newScore;
                }
            });
        }

        Data.GenAuxData.Plates.ForEach(p =>
        {
            var polys = p.Cells.SelectMany(c => c.PolyGeos).ToList();
            polys.ForEach(poly =>
            {
                if (poly.IsWater()) poly.Set(nameof(poly.Moisture), 1f, _key);
                else
                {
                    var moisture = plateMoistures[p] + Game.I.Random.RandfRange(-.1f, .1f);
                    
                    poly.Set(nameof(poly.Moisture), Mathf.Clamp(moisture, 0f, 1f), _key);
                }
            });
        });
    }

    private void BuildPolyVegetationTris()
    {
        //todo fix
        // foreach (var poly in Data.Planet.Polygons.Entities)
        // {
        //     poly.BuildTrisForAspects(Data.Models.Vegetation, _key);
        // }
    }
    
    private void BuildRivers()
    {
        var pathToSea = new Dictionary<MapPolygon, List<MapPolygon>>();
        var additional = new Dictionary<MapPolygon, float>();
        //todo check if double adding
        var polys = Data.Planet.Polygons.Entities.Distinct().ToList();
            
        polys.ForEach(p =>
        {
            if (p.IsWater()) return;
            var path = PathFinder<MapPolygon>.FindPathMultipleEnds(p,
                n => n.IsWater() || pathToSea.ContainsKey(n),
                n => n.Neighbors.Refs(), (n, m) => n.Roughness + m.Roughness);
            path.Reverse();
            if (path.First() != p) throw new Exception();
            pathToSea.Add(p, path);
            additional.Add(p, 0f);
        });
        foreach (var keyValuePair in pathToSea)
        {
            var origin = keyValuePair.Key;
            var path = keyValuePair.Value;
            var water = 10f;
            for (var i = 0; i < path.Count - 1; i++)
            {
                water += path[i].Moisture;
                path[i].GetBorder(path[i + 1], Data).IncrementFlow(water, _key);
            }
            while (path != null)
            {
                var last = path[path.Count - 1];
                if (last.IsWater())
                {
                    path = null;
                }
                else
                {
                    path = pathToSea[last];
                    additional[last] += water;
                }
            }
        }

        foreach (var keyValuePair in additional)
        {

            var origin = keyValuePair.Key;
            var add = keyValuePair.Value;
            var path = pathToSea[origin];
            for (var i = 0; i < path.Count - 1; i++)
            {
                path[i].GetBorder(path[i + 1], Data).IncrementFlow(add, _key);
            }
        }

        // var riverPolys = pathToSea.SelectMany(p => p.Value).Distinct().ToList();
        // riverPolys.ForEach(p => p.BuildTrisForAspect(LandformManager.River, _key));
    }
}