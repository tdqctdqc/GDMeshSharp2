using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        var equatorDistMultWeight = Data.GenMultiSettings.MoistureSettings.EquatorDistMoistureMultWeight.Value;
        Parallel.ForEach(Data.GenAuxData.Plates, p =>
        {
            var distFromEquator = Mathf.Abs(Data.Planet.Height / 2f - p.Center.y);
            var altMult = (1f - equatorDistMultWeight) + equatorDistMultWeight * (1f - distFromEquator / (Data.Planet.Height / 2f));
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
        var riverFlowPerMoisture = Data.GenMultiSettings.MoistureSettings.RiverFlowPerMoisture.Value;
        var baseRiverFlowCost = Data.GenMultiSettings.MoistureSettings.BaseRiverFlowCost.Value;
        var roughnessMult = Data.GenMultiSettings.MoistureSettings.RiverFlowCostRoughnessMult.Value;
        Parallel.ForEach(Data.Planet.Polygons.LandSea.Landmasses, doLandmass);
        
        void doLandmass(HashSet<MapPolygon> lm)
        {
            var sw = new Stopwatch();
            
            var landPolys = lm.Where(p => p.IsLand);
            var coastPolys = landPolys.Where(p => p.IsCoast());
            var innerPolys = landPolys.Except(coastPolys);
            
            sw.Start();
            var graph = DrainGraph<MapPolygon>.GetDrainGraph(
                innerPolys, 
                coastPolys,
                t => t.Moisture * riverFlowPerMoisture,
                (p, q) => (p.Roughness + q.Roughness) * roughnessMult
                            * p.GetOffsetTo(q, _key.Data).Length() + baseRiverFlowCost,
                Mathf.Inf, p => p.Neighbors.Entities());
            
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
                var flowIn = coast.Neighbors.Entities().Sum(n => coast.GetEdge(n, Data).MoistureFlow);
                var sea = coast.Neighbors.Entities().Where(n => n.IsWater())
                    .First();
                coast.GetEdge(sea, Data).SetFlow(flowIn, _key);
            }
        }
    }
}