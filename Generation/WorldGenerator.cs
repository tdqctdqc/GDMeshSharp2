using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class WorldGenerator
{
    public WorldData Data { get; private set; }
    private IDDispenser _id;
    private Vector2 _dim;
    private CreateWriteKey _key;
    public WorldGenerator(Vector2 dim)
    {
        _dim = dim;
        _id = new IDDispenser();
        Data = new WorldData(_dim);
        _key = new CreateWriteKey(Data);
    }
    public WorldData Generate()
    {
        var cellSize = 200f;
        var edgePointMargin = new Vector2(cellSize, cellSize);
        var points = PointsGenerator
            .GenerateConstrainedSemiRegularPoints
                (Data.Dimensions - edgePointMargin, cellSize, cellSize * .75f, false, true)
            .Select(v => v + edgePointMargin / 2f).ToList();

        var polygons 
            = VoronoiGenerator.GetVoronoiPolygons<GenPolygon>
                (
                    points, Data.Dimensions, true, cellSize,
                    (i, center) =>
                    {
                        _id.SetMin(i);
                        return new GenPolygon(i, center, Data.Dimensions.x, _key);
                    },
                    _key
                );
        
        Data.Setup(_key, _id);

        // Data.PlanetDomain.GeoPolygons.AddEntities(polygons, key);
        
        var geologyGenerator = new GeologyGenerator(Data, _id);
        geologyGenerator.GenerateTerrain(_key);

        var moistureGenerator = new MoistureGenerator(Data);
        moistureGenerator.Generate(_key);

        var locationGenerator = new LocationGenerator(Data);
        locationGenerator.Generate(_key);

        var regimeGen = new RegimeGenerator(Data);
        regimeGen.Generate();
        
        return Data;
    }
}