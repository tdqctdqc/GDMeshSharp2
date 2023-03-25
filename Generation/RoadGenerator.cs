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
        var allSegs = new Dictionary<MapPolygonEdge, RoadModel>();
        Parallel.ForEach(_data.Planet.Polygons.LandSea.Landmasses, lm =>
        {
            allSegs.AddRange(GenerateForLandmass(lm));
        });
        // _data.Planet.Polygons.LandSea.Landmasses.ForEach(lm =>
        // {
        //     allSegs.AddRange(GenerateForLandmass(lm));
        // });
        foreach (var kvp in allSegs)
        {
            RoadSegment.Create(kvp.Key, kvp.Value, _key);
        }
        // GenerateRoadNetwork();
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
            (p1, p2) => new Edge<MapPolygon>(p1, p2, (a, b) => a.Id > b.Id));
        
        var roadBuildDist = _data.GenMultiSettings.SocietySettings.RoadBuildDist.Value;
        var covered = new ConcurrentBag<Edge<MapPolygon>>();
        var segs = new ConcurrentDictionary<MapPolygonEdge, RoadModel>();
        
        GenerateRoadNetworkForTier(lm, 50f, .5f, 
            RoadModelManager.Railroad, 5000f,
            graph, covered, segs);
        
        GenerateRoadNetworkForTier(lm, 10f, .7f, 
            RoadModelManager.PavedRoad, 2000f,
            graph, covered, segs);
        
        GenerateRoadNetworkForTier(lm, 0f, .9f, 
            RoadModelManager.DirtRoad, 1000f,
            graph, covered, segs);
        
        return segs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
    private void GenerateRoadNetworkForTier(HashSet<MapPolygon> lm, float minSize, float minImprovementRatio, RoadModel road, float roadBuildDist,
        IReadOnlyGraph<MapPolygon, Edge<MapPolygon>> graph, ConcurrentBag<Edge<MapPolygon>> covered,
        ConcurrentDictionary<MapPolygonEdge, RoadModel> segs)
    {
        var settlementPolys = lm.Where(interested);
        if (settlementPolys.Count() < 3) return;
        float buildRoadEdgeCost(MapPolygon p1, MapPolygon p2)
        {
            if (p1.IsWater() || p2.IsWater()) return Mathf.Inf;
            var dist = p1.GetOffsetTo(p2, _data).Length();
            return dist * (p1.Roughness + p2.Roughness) / 2f;
        }

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
                
                var oldPath = PathFinder<MapPolygon>.FindPath(s1, s2, p => p.Neighbors.Entities(),
                    travelEdgeCost, (p1, p2) => p1.GetOffsetTo(p2, _data).Length());
                var oldCost = PathFinder<MapPolygon>.GetPathCost(oldPath, travelEdgeCost);
                
                var buildPath = PathFinder<MapPolygon>.FindPath(s1, s2, p => p.Neighbors.Entities(),
                    buildRoadEdgeCost, (p1, p2) => p1.GetOffsetTo(p2, _data).Length());
                var newCost = PathFinder<MapPolygon>.GetPathCost(oldPath, 
                    (p1, p2) => p1.GetOffsetTo(p2,_data).Length() / road.Speed);

                if (newCost > minImprovementRatio * oldCost)
                {
                    //todo in this case 'hook into' old path
                    continue;
                }

                for (var i = 0; i < buildPath.Count - 1; i++)
                {
                    var pathEdge = buildPath[i].GetEdge(buildPath[i + 1], _data);
                    if(segs.ContainsKey(pathEdge)) continue;
                    segs.TryAdd(pathEdge, road);
                }
            }
        };

        bool interested(MapPolygon p)
        {
            return _data.Society.Settlements.ByPoly.ContainsKey(p)
                   && p.GetSettlement(_data).Size >= minSize;
        }
    }
}
