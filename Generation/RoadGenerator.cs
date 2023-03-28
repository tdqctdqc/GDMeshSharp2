using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class RoadGenerator : Generator
{
    private GenData _data;
    private GenWriteKey _key;
    public override GenReport Generate(GenWriteKey key)
    {
        _key = key;
        _data = _key.GenData;
        var genReport = new GenReport(nameof(RoadGenerator));
        
        genReport.StartSection();
        var allSegs = new ConcurrentBag<IDictionary<MapPolygonEdge, RoadModel>>();
        Parallel.ForEach(_data.Planet.PolygonAux.LandSea.Landmasses, lm =>
        {
            allSegs.Add(GenerateForLandmass(lm));
        });
        foreach (var dic in allSegs)
        {
            foreach (var kvp in dic)
            {
                RoadSegment.Create(kvp.Key, kvp.Value, _key);
            }
        }
        genReport.StopSection(nameof(GenerateForLandmass));
        return genReport;
    }

    private IDictionary<MapPolygonEdge, RoadModel> GenerateForLandmass(HashSet<MapPolygon> lm)
    {
        var settlementPolys = lm.Where(p => p.HasSettlement(_data));
        if (settlementPolys.Count() < 3) return new Dictionary<MapPolygonEdge, RoadModel>();
        var first = lm.First();
        
        var graph = GraphGenerator.GenerateDelaunayGraph(settlementPolys.ToList(),
            s => first.GetOffsetTo(s, _data),
            (p1, p2) => new Edge<MapPolygon>(p1, p2, a => a.Id));
        
        
        var covered = new HashSet<Edge<MapPolygon>>();
        var segs = new Dictionary<MapPolygonEdge, RoadModel>();
        foreach (var graphEdge in graph.Edges)
        {
            
        }
        // GenerateLandmassRoadNetwork(lm, graph, covered, segs);

        // var regimeUnions = UnionFind.Find(_data.Planet.PolygonAux.BorderGraph, lm, (p, q) => p.Regime.RefId == q.Regime.RefId);
        // regimeUnions.ForEach(u => GenerateNationalRoadNetwork(u, graph, covered, segs));
        //
        return segs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private void GenerateNationalRoadNetwork(List<MapPolygon> union, 
        IReadOnlyGraph<MapPolygon, Edge<MapPolygon>> graph, HashSet<Edge<MapPolygon>> covered,
        Dictionary<MapPolygonEdge, RoadModel> segs)
    {
        // var first = union[0];
        var settlementPolys = union.Where(p => _data.Society.SettlementAux.ByPoly.ContainsKey(p))
            .OrderByDescending(p => p.GetSettlement(_data).Size)
            .ToList();
        // var numTopTier = Mathf.CeilToInt(settlementPolys.Count / 5);
        // var numMidTier = Mathf.CeilToInt(settlementPolys.Count / 3);
        // var numLowTier = Mathf.CeilToInt(settlementPolys.Count - (numTopTier + numMidTier));
        // if (numTopTier + numMidTier + numLowTier != settlementPolys.Count) throw new Exception();
        //
        // var topTier = settlementPolys.GetRange(0, numTopTier);
        // var midTier = settlementPolys.GetRange(numTopTier, numMidTier);
        // var lowTier = settlementPolys.GetRange(numTopTier + numMidTier, numLowTier);
        //
        // if (settlementPolys.Count < 3) return;
        //
        // var nationalGraph = GraphGenerator.GenerateDelaunayGraph(settlementPolys,
        //     p => first.GetOffsetTo(p, _data),
        //     (p, q) => p.GetOffsetTo(q, _data).Length());
        
        // var lowNetwork = GraphGenerator.GenerateSpanningGraph<MapPolygon>(
        //     nationalGraph, 5, int.MaxValue);
        //
        // lowNetwork.DoForEachEdge(
        //     (p, q, f) =>
        //     {
        //         var buildPath = PathFinder<MapPolygon>.FindPath(p, q, x => x.Neighbors.Entities(),
        //             BuildRoadEdgeCostNational, (p1, p2) => p1.GetOffsetTo(p2, _data).Length());
        //         if (buildPath == null) return;
        //         for (var i = 0; i < buildPath.Count - 1; i++)
        //         {
        //             var pathEdge = buildPath[i].GetEdge(buildPath[i + 1], _data);
        //             if(segs.ContainsKey(pathEdge)) continue;
        //             segs.Add(pathEdge, RoadModelManager.DirtRoad);
        //         }
        //     },
        //     p => p.Id
        // );
        
        BuildRoadNetwork(settlementPolys, Mathf.Inf, 
            RoadModelManager.DirtRoad,500f, graph, 
            covered, segs, false);
    }

    private void GenerateLandmassRoadNetwork(HashSet<MapPolygon> lm,
        IReadOnlyGraph<MapPolygon, Edge<MapPolygon>> graph, HashSet<Edge<MapPolygon>> covered,
        Dictionary<MapPolygonEdge, RoadModel> segs)
    {
        //todo make these dependent on city rank for regime rather than size
        GenerateRoadNetworkForMinSize(lm, 50f, 
            .4f, 
            RoadModelManager.Railroad, 2_000f,
            graph, covered, segs);
        
        GenerateRoadNetworkForMinSize(lm, 25f, 
            .6f, 
            RoadModelManager.PavedRoad, 1_000f,
            graph, covered, segs);
        
    }
    private void GenerateRoadNetworkForMinSize(HashSet<MapPolygon> lm, float minSize, float minImprovementRatio, 
        RoadModel road, float roadBuildDist,
        IReadOnlyGraph<MapPolygon, Edge<MapPolygon>> graph, HashSet<Edge<MapPolygon>> covered,
        Dictionary<MapPolygonEdge, RoadModel> segs)
    {
        var settlementPolys = lm.Where(p => _data.Society.SettlementAux.ByPoly.ContainsKey(p)
                                            && p.GetSettlement(_data).Size >= minSize);
        if (settlementPolys.Count() < 3) return;
        BuildRoadNetwork(settlementPolys, minImprovementRatio, road, roadBuildDist,
            graph, covered, segs, true);
    }

    private void BuildRoadNetwork(IEnumerable<MapPolygon> settlementPolys, float minImprovementRatio, RoadModel road, float roadBuildDist,
        IReadOnlyGraph<MapPolygon, Edge<MapPolygon>> graph, HashSet<Edge<MapPolygon>> covered,
        Dictionary<MapPolygonEdge, RoadModel> segs, bool international)
    {
        float travelEdgeCost(MapPolygon p1, MapPolygon p2)
        {
            var e = p1.GetEdge(p2, _data);
            if (p1.IsWater() || p2.IsWater()) return Mathf.Inf;
            var dist = p1.GetOffsetTo(p2, _data).Length();
            if (segs.ContainsKey(e))
            {
                return dist / segs[e].Speed;
            }
            else
            {
                return dist * (p1.Roughness + p2.Roughness);
            }
        }
        foreach (var s1 in settlementPolys)
        {
            foreach (var s2 in settlementPolys)
            {
                if (s1 == s2) continue;
                if (s1.GetOffsetTo(s2, _data).Length() > roadBuildDist) continue;
                var edge = new Edge<MapPolygon>(s1, s2, p => p.Id);
                if (covered.Contains(edge)) continue;
                covered.Add(edge);

                var oldPath = PathFinder.FindTravelPath(s1, s2, _data, travelEdgeCost);
                var oldCost = PathFinder.GetTravelPathCost(oldPath, _data, travelEdgeCost);
                
                var buildPath = PathFinder.FindRoadBuildPath(s1, s2, _data, international);
                var newCost = PathFinder.GetBuildPathCost(oldPath, _data);

                if (newCost > minImprovementRatio * oldCost)
                {
                    //todo in this case 'hook into' old path
                    continue;
                }

                for (var i = 0; i < buildPath.Count - 1; i++)
                {
                    var pathEdge = buildPath[i].GetEdge(buildPath[i + 1], _data);
                    if(segs.ContainsKey(pathEdge)) continue;
                    segs.Add(pathEdge, road);
                }
            }
        };

    }
    
}
