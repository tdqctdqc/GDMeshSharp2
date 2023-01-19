using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class WorldGenerator
{
    public WorldData Data { get; private set; }
    public WorldGenerator(Vector2 dim)
    {
        Data = new WorldData(dim);
    }
    public WorldData Generate()
    {
        var key = new CreateWriteKey(Data);
        var cellSize = 200f;

        var edgePointMargin = new Vector2(cellSize, cellSize);

        var points = PointsGenerator
            .GenerateConstrainedSemiRegularPoints
                (Data.Dimensions - edgePointMargin, cellSize, cellSize * .75f, false, true)
            .Select(v => v + edgePointMargin / 2f).ToList();
        var polygons 
            = VoronoiGenerator.GetVoronoiPolygons<GenPolygon>
                (points, Data.Dimensions, true, cellSize, 
                    (i, center) => new GenPolygon(i, center, Data.Dimensions.x, key),
                    key);
        // Data.PlanetDomain.GeoPolygons.AddEntities(polygons, key);
        
        var geologyGenerator = new GeologyGenerator(Data);
        geologyGenerator.GenerateTerrain(key);

        var moistureGenerator = new MoistureGenerator(Data);
        moistureGenerator.Generate(key);

        var locationGenerator = new LocationGenerator(Data);
        locationGenerator.Generate(key);

        var regimeGen = new RegimeGenerator(Data);
        regimeGen.Generate();
        
        return Data;
    }
}