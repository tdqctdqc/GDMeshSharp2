using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MoistureGenerator
{
    public WorldData Data { get; private set; }

    public MoistureGenerator(WorldData data)
    {
        Data = data;
    }

    public void Generate()
    {
        SetPolyMoistures();
        BuildVegetationTris();
        BuildRivers();
    }
    private void SetPolyMoistures()
    {
        var massMoistures = new Dictionary<GeoMass, float>();
        
        Data.Masses.ForEach(m =>
        {
            var distFromEquator = Mathf.Abs(Data.Dimensions.y / 2f - m.Center.y);
            var altMult = .5f + .5f * (1f - distFromEquator / (Data.Dimensions.y / 2f));
            var polyGeos = m.Plates.SelectMany(p => p.Cells).SelectMany(c => c.PolyGeos).ToList();
            var count = polyGeos.Count;
            var waterCount = polyGeos.Where(g => g.IsWater()).Count();
            var score = altMult * waterCount / count;
            massMoistures.Add(m, score);
            
        });


        for (int i = 0; i < 2; i++)
        {
            diffuse();
        }

        void diffuse()
        {
            Data.Masses.ForEach(m =>
            {
                var oldScore = massMoistures[m];

                var newScore = m.Neighbors.Select(n => massMoistures[n]).Average();

                if (newScore > oldScore)
                {
                    massMoistures[m] = newScore;
                }
            });
        }

        Data.Masses.ForEach(m =>
        {
            var polyGeos = m.Plates.SelectMany(p => p.Cells).SelectMany(c => c.PolyGeos).ToList();
            polyGeos.ForEach(p =>
            {
                if (p.IsWater()) p.SetMoisture(1f);
                else
                {
                    var moisture = massMoistures[m] + Game.I.Random.RandfRange(-.1f, .1f);
                    
                    p.SetMoisture(Mathf.Clamp(moisture, 0f, 1f));
                }
            });
        });
    }

    private void BuildVegetationTris()
    {
        Data.Vegetation.BuildTris(Data.GeoPolygons.ToHashSet(), Data);
    }

    
    private void BuildRivers()
    {
        var pathToSea = new Dictionary<GeoPolygon, List<GeoPolygon>>();
        var additional = new Dictionary<GeoPolygon, float>();
        //todo check if double adding
        var polys = Data.GeoPolygons.Distinct().ToList();
            
        polys.ForEach(p =>
        {
            if (p.IsWater()) return;
            var path = PathFinder<GeoPolygon>.FindPathMultipleEnds(p,
                n => n.IsWater() || pathToSea.ContainsKey(n),
                n => n.GeoNeighbors, (n, m) => n.Roughness + m.Roughness);
            path.Reverse();
            if (path.First() != p) throw new Exception();
            pathToSea.Add(p, path);
            additional.Add(p, 0f);
        });
        var iter1 = 0;
        foreach (var keyValuePair in pathToSea)
        {
            // if(iter1 % 100 == 0) GD.Print($"{iter1} / {pathToSea.Count}");
            iter1++;

            var origin = keyValuePair.Key;
            var path = keyValuePair.Value;
            var water = 10f;
            for (var i = 0; i < path.Count - 1; i++)
            {
                water += path[i].Moisture;
                path[i].GetGeoPolyBorder(path[i + 1]).IncrementFlow(water);
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

        var iter2 = 0;
        foreach (var keyValuePair in additional)
        {
            // if(iter2 % 100 == 0) GD.Print($"{iter2} / {additional.Count}");
            iter2++;

            var origin = keyValuePair.Key;
            var add = keyValuePair.Value;
            var path = pathToSea[origin];
            for (var i = 0; i < path.Count - 1; i++)
            {
                path[i].GetGeoPolyBorder(path[i + 1]).IncrementFlow(add);
            }
        }
        Data.Landforms.BuildTrisForAspect(LandformManager.River, Data, 
            pathToSea.SelectMany(p => p.Value).Distinct().ToList());

    }
}