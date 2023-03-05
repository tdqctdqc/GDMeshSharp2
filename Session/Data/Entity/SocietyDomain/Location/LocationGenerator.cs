using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Priority_Queue;

public class LocationGenerator 
{
    public GenData Data { get; private set; }
    private GenWriteKey _key;
    private IdDispenser _id;
    public LocationGenerator(GenData data)
    {
        Data = data;
    }

    public void Generate(GenWriteKey key, IdDispenser id)
    {
        _key = key;
        _id = id;
        GenerateCities();
        GenerateRoadNetwork();
    }
    
    private void GenerateCities()
    {
        var landPolys = Data.Planet.Polygons.Entities.Where(p => p.IsLand());
        var unions = UnionFind<MapPolygon>.DoUnionFind(landPolys.ToList(), (p, q) => p.Regime.Entity() == q.Regime.Entity(),
            p => p.Neighbors.Refs());
        unions.ForEach(u => GenerateCitiesForRegimeChunk(u));
    }

    private void GenerateCitiesForRegimeChunk(List<MapPolygon> polys)
    {
        float minSettlementScore = .5f;
        float popScore(MapPolygon poly)
        {
            return 2f * (poly.Moisture - (poly.Roughness * .5f));
        }

        float settlementDesireability(MapPolygon poly)
        {
            var res = popScore(poly);
            if (poly.GetTerrainTris(Data).Tris.Any(t => t.Landform == LandformManager.River))
            {
                res += 1f;
            }
            if (poly.IsCoast())
            {
                res += 1f;
            }
            return res;
        }

        var score = polys.Sum(popScore);

        var validSettlementPolys = polys.Where(p => p.GetTerrainTris(Data).Tris.Any(t =>
        {
            return t.Landform != LandformManager.Mountain
                   && t.Landform != LandformManager.Peak
                   && t.Landform != LandformManager.River;
        }));
        
        var polyQueue = new SimplePriorityQueue<MapPolygon>();
        foreach (var v in validSettlementPolys)
        {
            polyQueue.Enqueue(v, -settlementDesireability(v));
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

        var numSettlements = Mathf.Min(settlementPolys.Count, settlementSizes.Count);
        
        
        for (var i = 0; i < numSettlements; i++)
        {
            var p = settlementPolys[i];
            var size = settlementSizes[i];
            Settlement.Create(_id.GetID(), NameGenerator.GetName(), p, size, _key);
            var availTris = p.GetTerrainTris(Data).Tris
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
        //get points of offsets from given poly
        //generate delaunay triangulation from that, build graph, find edge paths
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
                    var border = path[i].GetBorder(path[i + 1], Data);
                    if(Data.Society.Roads.ByBorderId.ContainsKey(border.Id)) continue;
                    var road = RoadSegment.Create(_id.GetID(), border, _key);
                }
            }
        });
    }
}