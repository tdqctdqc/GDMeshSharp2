using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Priority_Queue;

public class LocationGenerator : Generator
{
    public GenData Data { get; private set; }
    private GenWriteKey _key;
    private IdDispenser _id;
    public LocationGenerator()
    {
    }

    public override GenReport Generate(GenWriteKey key)
    {
        var report = new GenReport(GetType().Name);
        _key = key;
        _id = key.IdDispenser;
        Data = key.GenData;
        report.StartSection();
        GenerateCities();
        report.StopSection("Generating Cities");
        
        report.StartSection();
        GenerateRoadNetwork();
        report.StopSection("Generating Road Network");

        return report;
    }
    
    private void GenerateCities()
    {
        var landPolys = Data.Planet.Polygons.Entities.Where(p => p.IsLand());
        var unions = UnionFind.Find(landPolys.ToList(), (p, q) => p.Regime.Entity() == q.Regime.Entity(),
            p => p.Neighbors.Refs());

        var dic = new ConcurrentDictionary<List<MapPolygon>, List<float>>();
        
        Parallel.ForEach(unions, u =>
        {
            var res = PregenerateSettlements(u);
            dic.TryAdd(res.settlementPolys, res.settlementSizes);
        });
        foreach (var kvp in dic)
        {
            CreateSettlements(kvp.Key, kvp.Value);
        }
        Parallel.ForEach(dic, kvp =>
        {
            SetUrbanTris(kvp.Key, kvp.Value);
        });
    }
    
    private (List<MapPolygon> settlementPolys, List<float> settlementSizes)
        PregenerateSettlements(List<MapPolygon> polys)
    {
        float minSettlementScore = .5f;
        var score = polys.Sum(PopScore);
        
        var polyQueue = new SimplePriorityQueue<MapPolygon>();
        for (var i = 0; i < polys.Count; i++)
        {
            var p = polys[i];
            if (p.TerrainTris.Tris.Any(t => t.Landform != LandformManager.Mountain
                                            && t.Landform != LandformManager.Peak
                                            && t.Landform != LandformManager.River))
            {
                polyQueue.Enqueue(p, -SettlementDesireability(p));
            }
        }
        
        var settlementPolys = new List<MapPolygon>();
        var forbidden = new HashSet<MapPolygon>();

        while (polyQueue.Count > 0)
        {
            var poly = polyQueue.Dequeue();
            if (forbidden.Contains(poly)) continue;
            foreach (var n in poly.Neighbors.Refs())
            {
                forbidden.Add(n);
            }
            settlementPolys.Add(poly);
        }

        var num = 1;
        var settlementSizes = new List<float>();
        while (score > minSettlementScore)
        {
            var size = (score / 2f) / num;
            for (var i = 0; i < num; i++)
            {
                settlementSizes.Add(size);
            }
            score *= .5f;
            num *= 2;
        }

        return (settlementPolys, settlementSizes);
    }

    private void CreateSettlements(List<MapPolygon> settlementPolys, List<float> settlementSizes)
    {
        var numSettlements = Mathf.Min(settlementPolys.Count, settlementSizes.Count);
        for (var i = 0; i < numSettlements; i++)
        {
            var p = settlementPolys[i];
            var size = settlementSizes[i];
            Settlement.Create(_id.GetID(), 
                "doot", 
                // NameGenerator.GetName(), //todo fix this
                p, size, _key);
        }
    }
    private void SetUrbanTris(List<MapPolygon> settlementPolys, List<float> settlementSizes)
    {
        var numSettlements = Mathf.Min(settlementPolys.Count, settlementSizes.Count);

        for (var i = 0; i < numSettlements; i++)
        {
            var p = settlementPolys[i];
            var size = settlementSizes[i];

            var availTris = p.TerrainTris.Tris
                .Where(t => t.Landform != LandformManager.River
                    && t.Landform != LandformManager.Mountain
                    && t.Landform != LandformManager.Peak)
                .OrderBy(t => t.GetCentroid().LengthSquared());
            
            var numUrbanTris = Mathf.Min(availTris.Count(), Mathf.CeilToInt(size / 2f));
            for (var j = 0; j < numUrbanTris; j++)
            {
                availTris.ElementAt(j).SetLandform(LandformManager.Urban, _key);
                availTris.ElementAt(j).SetVegetation(VegetationManager.Barren, _key);
            }
        }
    }
    private void GenerateRoadNetwork()
    {
        Data.LandSea.Landmasses.ForEach(lm =>
        {
            var settlements = lm.Where(p => Data.Society.Settlements.ByPoly.ContainsKey(p));
            if (settlements.Count() == 0) return;
            var first = settlements.First();
            var points = settlements.Select(s => first.GetOffsetTo(s, Data)).ToList();
            if (points.Count < 3) return;

            var graph = GraphGenerator.GenerateDelaunayGraph(settlements.ToList(),
                s => first.GetOffsetTo(s, Data),
                (p1, p2) => new Edge<MapPolygon>(p1, p2, (a, b) => a.Id > b.Id));

            float edgeCost(MapPolygon p1, MapPolygon p2)
            {
                if (p1.IsWater() || p2.IsWater()) return Mathf.Inf;
                return p1.Roughness + p2.Roughness;
            }
            foreach (var e in graph.Edges)
            {
                if (e.T1.GetOffsetTo(e.T2, Data).Length() > 1000f) continue;
                var path = PathFinder<MapPolygon>.FindPath(e.T1, e.T2, p => p.Neighbors.Refs(),
                    edgeCost, (p1, p2) => p1.GetOffsetTo(p2, Data).Length());

                for (var i = 0; i < path.Count - 1; i++)
                {
                    var edge = path[i].GetEdge(path[i + 1], Data);

                    if(Data.Society.Roads.ByEdgeId.ContainsKey(edge.Id)) continue;
                    RoadSegment.Create(_id.GetID(), edge, _key);
                }
            }
        });
    }
    
    private float PopScore(MapPolygon poly)
    {
        return 2f * (poly.Moisture - (poly.Roughness * .5f));
    }

    private float SettlementDesireability(MapPolygon poly)
    {
        var res = PopScore(poly);
        if (poly.TerrainTris.Tris.Any(t => t.Landform == LandformManager.River))
        {
            res += 1f;
        }
        if (poly.IsCoast())
        {
            res += 1f;
        }
        return res;
    }
}