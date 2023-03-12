using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class MoistureGenerator : Generator
{
    public GenData Data { get; private set; }
    private GenWriteKey _key;
    private IdDispenser _id;
    public MoistureGenerator()
    {
    }

    public override GenReport Generate(GenWriteKey key)
    {
        var report = new GenReport(GetType().Name);
        _key = key;
        Data = key.GenData;
        _id = key.IdDispenser;
        report.StartSection();
        SetPolyMoistures();
        report.StopSection("SetPolyMoistures");
        
        report.StartSection();
        BuildRiversDrainGraph();
        report.StopSection("BuildRiversDrainGraph");
        return report;
    }
    private void SetPolyMoistures()
    {
        var plateMoistures = new ConcurrentDictionary<GenPlate, float>();
        
        Parallel.ForEach(Data.GenAuxData.Plates, p =>
        {
            var distFromEquator = Mathf.Abs(Data.Planet.Height / 2f - p.Center.y);
            var altMult = .5f + .5f * (1f - distFromEquator / (Data.Planet.Height / 2f));
            var polyGeos = p.Cells.SelectMany(c => c.PolyGeos).ToList();
            var count = polyGeos.Count;
            var waterCount = polyGeos.Where(g => g.IsWater()).Count();
            var score = altMult * waterCount / count;
            plateMoistures.TryAdd(p, score);
        });


        float maxFriction = 0f;
        float averageFriction = 0f;
        int iter = 0;
        for (int i = 0; i < 3; i++)
        {
            diffuse();
        }
        Parallel.ForEach(Data.GenAuxData.Plates, setPlateMoistures);

        void setPlateMoistures(GenPlate plate)
        {
            var polys = plate.Cells.SelectMany(c => c.PolyGeos).ToList();
            foreach (var cell in plate.Cells)
            {
                foreach (var poly in cell.PolyGeos)
                {
                    if (poly.IsWater()) poly.Set(nameof(poly.Moisture), 1f, _key);
                    else
                    {
                        var moisture = plateMoistures[plate] + Game.I.Random.RandfRange(-.1f, .1f);
                    
                        poly.Set(nameof(poly.Moisture), Mathf.Clamp(moisture, 0f, 1f), _key);
                    }
                }
            }
        }
        
        
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

        
    }

    private void BuildRiversDrainGraph()
    {
        var lms = Data.LandSea.Landmasses;
        int iter = 0;

        Parallel.ForEach(Data.LandSea.Landmasses, doLandmass);
        
        void doLandmass(HashSet<MapPolygon> lm)
        {
            var landPolys = lm.Where(p => p.IsLand());
            var coastPolys = landPolys.Where(p => p.IsCoast());
            var innerPolys = landPolys.Except(coastPolys);
            var graph = DrainGraph<MapPolygon>.GetDrainGraph(
                innerPolys, 
                coastPolys,
                t => t.Moisture * 10f,
                (p, q) => (p.Roughness + q.Roughness) * p.GetOffsetTo(q, _key.Data).Length() + 100f,
                Mathf.Inf, p => p.Neighbors.Refs());
            graph.Elements.ForEach(e =>
            {
                var ns = graph.GetNeighbors(e);
                foreach (var n in ns)
                {
                    if (e.Id < n.Id) continue;
                    var edge = e.GetEdge(n, Data);
                    var flow = graph.GetEdge(e, n);
                    edge.SetFlow(graph.GetEdge(e, n), _key);
                }
            });
            
            foreach (var coast in coastPolys)
            {
                var flowIn = coast.Neighbors.Refs().Sum(n => coast.GetEdge(n, Data).MoistureFlow);
                var sea = coast.Neighbors.Refs().Where(n => n.IsWater())
                    .First();
                coast.GetEdge(sea, Data).SetFlow(flowIn, _key);
            }
        }
    }
    private void BuildRiversOrTools()
    {
        var lms = Data.LandSea.Landmasses;
        int iter = 0;
        foreach (var lm in lms)
        {
            var moistureTot = Mathf.CeilToInt(lm.Sum(l => l.Moisture));

            var landPolys = lm
                .Where(p => p.IsLand()).ToList();
            var coastPolys = landPolys.Where(p => p.IsCoast()).ToHashSet();


            Func<MapPolygon, MapPolygon, int> getCost = (p, q) =>
            {
                return Mathf.CeilToInt((p.Roughness + q.Roughness) * p.GetOffsetTo(q, _key.Data).Length());
            };
            Func<MapPolygon, int> getSupply = p =>
            {
                if (coastPolys.Contains(p)) return int.MinValue;
                return Game.I.Random.Randf() > .01f ? 0 : 1;
            };
            var sol = OrToolsExt.SolveMinFlow(
                landPolys, 
                p => p.Neighbors.Refs(),
                (p, q) => coastPolys.Contains(p) ? 0 : 100,
                getCost,
                getSupply
            );
        
            foreach (var node in sol.Nodes)
            {
                foreach (var n in node.Neighbors)
                {
                    if (node.Element.Id < n.Id) continue;
                    var edge = node.Element.GetEdge(n, Data);
                    var cost = node.GetEdgeCost(n);
                    edge.IncrementFlow(cost, _key);
                }
            }
        }
        
        
    }
    private void BuildRivers()
    {
        var pathToSea = new Dictionary<MapPolygon, List<MapPolygon>>();
        var additional = new Dictionary<MapPolygon, float>();
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
                path[i].GetEdge(path[i + 1], Data).IncrementFlow(water, _key);
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
                path[i].GetEdge(path[i + 1], Data).IncrementFlow(add, _key);
            }
        }

        // var riverPolys = pathToSea.SelectMany(p => p.Value).Distinct().ToList();
        // riverPolys.ForEach(p => p.BuildTrisForAspect(LandformManager.w, _key));
    }
}