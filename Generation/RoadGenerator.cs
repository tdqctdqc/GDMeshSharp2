using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DelaunatorSharp;
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
        var allSegs = new ConcurrentBag<IDictionary<Edge<MapPolygon>, RoadModel>>();
        Parallel.ForEach(_data.Planet.PolygonAux.LandSea.Landmasses, lm =>
        {
            allSegs.Add(GenerateForLandmass(lm));
        });
        foreach (var dic in allSegs)
        {
            foreach (var kvp in dic)
            {
                var edge = kvp.Key;
                var polyEdge = edge.T1.GetEdge(edge.T2, _data);
                RoadSegment.Create(polyEdge, kvp.Value, _key);
            }
        }
        genReport.StopSection(nameof(GenerateForLandmass));
        return genReport;
    }

    private IDictionary<Edge<MapPolygon>, RoadModel> GenerateForLandmass(HashSet<MapPolygon> lm)
    {
        var settlementPolys = lm.Where(p => p.HasSettlement(_data));
        if (settlementPolys.Count() < 3) return new Dictionary<Edge<MapPolygon>, RoadModel>();
        var first = lm.First();
        
        // var allSettlementsGraph = GraphGenerator.GenerateDelaunayGraph(settlementPolys.ToList(),
        //     s => first.GetOffsetTo(s, _data),
        //     (p1, p2) => new Edge<MapPolygon>(p1, p2, a => a.Id));

        bool rail(Settlement s)
        {
            return s.Size >= 50f;
        }
        bool paved(Settlement s)
        {
            return s.Size >= 25f;
        }
        bool dirt(Settlement s)
        {
            return s.Size >= 5f;
        }
        var covered = new HashSet<Edge<MapPolygon>>();
        var segs = new Dictionary<Edge<MapPolygon>, RoadModel>();
        
        var railSettlements = settlementPolys.Where(p => rail(p.GetSettlement(_data))).ToList();
        if(railSettlements.Count > 2)
        {
            var railGraph = GraphGenerator.GenerateDelaunayGraph(railSettlements,
                s => first.GetOffsetTo(s, _data),
                (p1, p2) => new Edge<MapPolygon>(p1, p2, a => a.Id));
            BuildRoadNetworkLocal(RoadModelManager.Railroad, 2000f, .7f,
                railGraph, covered, segs, true);
        }
        
        var pavedSettlements = settlementPolys.Where(p => paved(p.GetSettlement(_data))).ToList();
        if(pavedSettlements.Count > 2)
        {
            var pavedGraph = GraphGenerator.GenerateDelaunayGraph(pavedSettlements,
                s => first.GetOffsetTo(s, _data),
                (p1, p2) => new Edge<MapPolygon>(p1, p2, a => a.Id));
            BuildRoadNetworkLocal(RoadModelManager.PavedRoad, 1000f, .7f,
                pavedGraph, covered, segs, true);
        }
        
        var dirtSettlements = settlementPolys.Where(p => dirt(p.GetSettlement(_data))).ToList();
        if(dirtSettlements.Count > 2)
        {
            var dirtGraph = GraphGenerator.GenerateDelaunayGraph(dirtSettlements,
                s => first.GetOffsetTo(s, _data),
                (p1, p2) => new Edge<MapPolygon>(p1, p2, a => a.Id));
            BuildRoadNetworkLocal(RoadModelManager.DirtRoad, 500f, .7f,
                dirtGraph, covered, segs, true);
        }
        return segs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
    private void GenerateRoadNetworkForMinSize(HashSet<MapPolygon> lm, float minSize, float minImprovementRatio, 
        RoadModel road, float roadBuildDist,
        IReadOnlyGraph<MapPolygon, Edge<MapPolygon>> graph, HashSet<Edge<MapPolygon>> covered,
        Dictionary<Edge<MapPolygon>, RoadModel> segs)
    {
        var settlementPolys = lm.Where(p => _data.Society.SettlementAux.ByPoly.ContainsKey(p)
                                            && p.GetSettlement(_data).Size >= minSize);
        if (settlementPolys.Count() < 3) return;
        BuildRoadNetwork(settlementPolys, minImprovementRatio, road, roadBuildDist,
            graph, covered, segs, true);
    }
    
    private void BuildRoadNetwork(IEnumerable<MapPolygon> settlementPolys, float minImprovementRatio, RoadModel road, float roadBuildDist,
        IReadOnlyGraph<MapPolygon, Edge<MapPolygon>> graph, HashSet<Edge<MapPolygon>> covered,
        Dictionary<Edge<MapPolygon>, RoadModel> segs, bool international)
    {
        
        foreach (var s1 in settlementPolys)
        {
            foreach (var s2 in settlementPolys)
            {
                if (s1 == s2) continue;
                if (s1.GetOffsetTo(s2, _data).Length() > roadBuildDist) continue;
                var edge = new Edge<MapPolygon>(s1, s2, p => p.Id);
                if (covered.Contains(edge)) continue;
                covered.Add(edge);
                TryBuildNewPath(s1, s2, international, minImprovementRatio, road,
                    segs, covered, travelEdgeCost, buildRoadEdgeCost);
            }
        };
        float travelEdgeCost(MapPolygon p1, MapPolygon p2)
        {
            var e = new Edge<MapPolygon>(p1, p2, p => p.Id);
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
        float buildRoadEdgeCost(MapPolygon p1, MapPolygon p2)
        {
            if (p1.IsWater() || p2.IsWater()) return Mathf.Inf;
            if (international == false && p1.Regime.RefId != p2.Regime.RefId) return Mathf.Inf;
            if (segs.TryGetValue(new Edge<MapPolygon>(p1, p2, p => p.Id), out var rm))
            {
                if (rm == road) return 0f;
            }
            var dist = p1.GetOffsetTo(p2, _data).Length();
            return dist * (p1.Roughness + p2.Roughness) / 2f;
        }
    }

    private void TryBuildNewPath(MapPolygon s1, MapPolygon s2, bool international, float minImprovementRatio,
         RoadModel road, Dictionary<Edge<MapPolygon>, RoadModel> segs, HashSet<Edge<MapPolygon>> covered, 
         Func<MapPolygon, MapPolygon, float> travelEdgeCost = null, Func<MapPolygon, MapPolygon, float> buildEdgeCost = null)
    {
        var oldPath = PathFinder.FindTravelPath(s1, s2, _data, travelEdgeCost);
        var oldCost = PathFinder.GetTravelPathCost(oldPath, _data, travelEdgeCost);
                
        var buildPath = PathFinder.FindRoadBuildPath(s1, s2, road, _data, 
            international, buildEdgeCost);
        var newCost = PathFinder.GetBuildPathCost(oldPath, road, _data, 
            international, buildEdgeCost);

        if (newCost > minImprovementRatio * oldCost)
        {
            //todo in this case 'hook into' old path
            return;
        }

        for (var i = 0; i < buildPath.Count - 1; i++)
        {
            var pathEdge = new Edge<MapPolygon>(buildPath[i], buildPath[i + 1], mp => mp.Id); 
            buildPath[i].GetEdge(buildPath[i + 1], _data);
            covered.Add(pathEdge);
            if(segs.ContainsKey(pathEdge)) continue;
            segs.Add(pathEdge, road);
        }
    }
    private void BuildRoadNetworkLocal(RoadModel road, float roadBuildDist, float minImprovementRatio,
        IReadOnlyGraph<MapPolygon, Edge<MapPolygon>> graph, HashSet<Edge<MapPolygon>> covered,
        Dictionary<Edge<MapPolygon>, RoadModel> segs, bool international)
    {
        var distSqr = roadBuildDist * roadBuildDist;
        foreach (var e in graph.Edges)
        {
            if (covered.Contains(e)) continue;
            covered.Add(e);
            var s1 = e.T1;
            var s2 = e.T2;
            if (s1.GetOffsetTo(s2, _data).LengthSquared() > distSqr) continue;

            var buildPath = PathFinder.FindRoadBuildPath(s1, s2, road, _data, international);
            for (var i = 0; i < buildPath.Count - 1; i++)
            {
                var pathEdge = new Edge<MapPolygon>(buildPath[i], buildPath[i + 1], mp => mp.Id); 
                buildPath[i].GetEdge(buildPath[i + 1], _data);
                covered.Add(pathEdge);
                if(segs.ContainsKey(pathEdge)) continue;
                segs.Add(pathEdge, road);
            }
        }
    }
}
