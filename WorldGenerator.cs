using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class WorldGenerator
{
    public static WorldData Generate()
    {
        var dim = new Vector2(16000, 8000);
        var cellSize = 200f;

        var edgePointMargin = new Vector2(cellSize, cellSize);
        var worldData = new WorldData(dim);

        var points = PointsGenerator.GenerateConstrainedSemiRegularPoints(dim - edgePointMargin, cellSize, cellSize * .75f, false, true)
            .Select(v => v + edgePointMargin / 2f).ToList();
        var polygons = VoronoiGenerator.GetVoronoiPolygons<GeologyPolygon>(points, dim, true, cellSize, (i, center) => new GeologyPolygon(i, center));
        worldData.GeoPolygons.AddRange(polygons);
        
        var geologyGenerator = new GeologyGenerator(worldData);
        geologyGenerator.GenerateTerrain();
        return worldData;
    }
}