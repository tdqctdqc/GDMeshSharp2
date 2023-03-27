using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class EdgeDisturber
{
    private static GenWriteKey _key;
    
    public static void SplitEdges(IReadOnlyCollection<MapPolygon> polys, GenWriteKey key, float minLength)
    {
        _key = key;
        var edges = new HashSet<MapPolygonEdge>();

        void AddEdge(MapPolygon p1, MapPolygon p2)
        {
            edges.Add(p1.GetEdge(p2, key.Data));
        }

        int iter = 0;
        foreach (var poly in polys)
        {
            foreach (var n in poly.Neighbors.Entities())
            {
                var edge = poly.GetEdge(n, key.Data);
                if (edges.Contains(edge)) continue;
                edges.Add(edge);

                if (edge.HighSegsRel().Segments.Any(s => s.Length() > minLength * 2f))
                {
                    iter++;
                    edge.SplitToMinLength(minLength, _key);
                }
            }
        }
    }
    
    public static void DisturbEdges(IReadOnlyCollection<MapPolygon> polys, Vector2 dimensions, GenWriteKey key)
    {
        _key = key;
        var noise = new OpenSimplexNoise();
        noise.Period = dimensions.x;
        noise.Octaves = 2;
        var disturbedEdges = new HashSet<Vector2>();
        for (var i = 0; i < polys.Count; i++)
        {
            var poly = polys.ElementAt(i);
            for (var j = 0; j < poly.Neighbors.Count(); j++)
            {
                var nPoly = poly.Neighbors.Entities().ElementAt(j);
                if (poly.Id > nPoly.Id)
                {
                    // DisturbEdge(_key.Data, poly, nPoly, noise);
                }
            }
        }
    }
}
