using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class WorldGenerator
{
    public WorldData Data { get; private set; }
    private IDDispenser _id;
    private Vector2 _dim;
    private GenWriteKey _key;
    public WorldGenerator(Vector2 dim)
    {
        _dim = dim;
        _id = new IDDispenser();
        Data = new WorldData(_dim);
        _key = new GenWriteKey(Data);
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
        
        var geologyGenerator = new GeologyGenerator(Data, _id);
        geologyGenerator.GenerateTerrain(_key);

        var moistureGenerator = new MoistureGenerator(Data, _id);
        moistureGenerator.Generate(_key);

        var locationGenerator = new LocationGenerator(Data);
        locationGenerator.Generate(_key, _id);

        var regimeGen = new RegimeGenerator(Data, _id, _key);
        regimeGen.Generate();
        
        return Data;
    }
}