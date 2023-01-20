using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LocationGenerator 
{
    public WorldData Data { get; private set; }
    private CreateWriteKey _key;
    public LocationGenerator(WorldData data)
    {
        Data = data;
    }

    public void Generate(CreateWriteKey key)
    {
        _key = key;
        GenerateCities();
        GenerateRoadNetwork();
    }
    
    private void GenerateCities()
    {
        var minScoreForSettlement = 2f;
        Data.Plates.ForEach(plate =>
        {
            var landPolys = plate.Cells.SelectMany(c => c.PolyGeos.Refs)
                .Where(p => p.IsLand())
                .Where(p => p.Roughness < LandformManager.Mountain.MinRoughness);
            if (landPolys.Count() == 0) return;
            var score = landPolys.Select(popScore).Sum();
            var settlementScores = new List<float>();
            var queue = new Queue<float>();
            queue.Enqueue(score);
            while (queue.Count > 0
                   && queue.Peek() >= minScoreForSettlement 
                   && settlementScores.Count <= landPolys.Count() / 2
                   )
            {
                var thisScore = queue.Dequeue();
                var thisSettlementScore = Mathf.Max(thisScore / 2f, minScoreForSettlement);
                settlementScores.Add(thisSettlementScore);
                var subScore = (thisScore - thisSettlementScore);
                if (subScore >= minScoreForSettlement * 3f)
                {
                    queue.Enqueue(subScore);
                    queue.Enqueue(subScore);
                    queue.Enqueue(subScore);
                }
                else
                {
                    var numSub = Mathf.FloorToInt(subScore / minScoreForSettlement);
                    for (int i = 0; i < numSub; i++)
                    {
                        queue.Enqueue(subScore / numSub);
                    }
                }
            }

            var settlementPolys = landPolys.GetNRandomElements(settlementScores.Count);
            for (var i = 0; i < settlementPolys.Count; i++)
            {
                settlementPolys[i].Set(nameof(GenPolygon.SettlementSize), settlementScores[i], _key);
                    
                    
                Data.Locations.Settlements.Add(new Settlement(settlementPolys[i], settlementScores[i]));
            }
        });
        Data.Landforms.BuildTrisForAspect(LandformManager.Urban, Data);

        float popScore(GenPolygon p)
        {
            return Mathf.Clamp(p.Moisture - p.Roughness * .3f, .2f, 1f);
        }
    }

    private void GenerateRoadNetwork()
    {
        //get points of offsets from given poly
        //generate delaunay triangulation from that, build graph, find edge paths
        Data.LandSea.Landmasses.ForEach(lm =>
        {
            var settlements = lm.Where(p => p.SettlementSize > 0f);
            if (settlements.Count() == 0) return;
            var first = settlements.First();
            var points = settlements.Select(s => first.GetOffsetTo(s, Data.Dimensions.x)).ToList();
            if (points.Count < 3) return;

            var graph = GraphGenerator.GenerateDelaunayGraph(settlements.ToList(),
                s => first.GetOffsetTo(s, Data.Dimensions.x),
                (p1, p2) => new Edge<GenPolygon>(p1, p2, (a, b) => a.Id > b.Id));

            float edgeCost(GenPolygon p1, GenPolygon p2)
            {
                if (p1.IsWater() || p2.IsWater()) return Mathf.Inf;
                return p1.Roughness + p2.Roughness;
            }
            foreach (var e in graph.Edges)
            {
                if (e.T1.GetOffsetTo(e.T2, Data.Dimensions.x).Length() > 1000f) continue;
                var path = PathFinder<GenPolygon>.FindPath(e.T1, e.T2, p => p.GeoNeighbors.Refs,
                    edgeCost, (p1, p2) => p1.GetOffsetTo(p2, Data.Dimensions.x).Length());
                for (var i = 0; i < path.Count - 1; i++)
                {
                    var edge = new Edge<GenPolygon>(path[i], path[i + 1], (p, q) => p.Id > q.Id);
                    Data.Locations.Roads.Add(edge);
                }
            }
        });
    }
}