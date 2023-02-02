using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class WorldGenerator
{
    public GenData Data { get; private set; }
    private IDDispenser _id;
    private Vector2 _dim;
    private GenWriteKey _key;
    public WorldGenerator(Vector2 dim)
    {
        _dim = dim;
        _id = new IDDispenser();
        Data = new GenData();
        _key = new GenWriteKey(Data);
    }
    public GenData Generate()
    {
        var cellSize = 200f;
        var edgePointMargin = new Vector2(cellSize, cellSize);

        var planetInfo = PlanetInfo.Create(_dim, _id.GetID(), _key);
        var points = PointsGenerator
            .GenerateConstrainedSemiRegularPoints
                (_dim - edgePointMargin, cellSize, cellSize * .75f, false, true)
            .Select(v => v + edgePointMargin / 2f).ToList();

        PolygonGenerator.GenerateMapPolygons
        (
            points, _dim, true, cellSize,
            _id,
            _key
        );
        
        EdgeDisturber.DisturbEdges(Data.Planet.Polygons.Entities, 
            Data.Planet.PlanetInfo.Value.Dimensions, _key);
        Data.Events.FinalizedPolyShapes?.Invoke();

        var geologyGenerator = new GeologyGenerator(Data, _id);
        geologyGenerator.GenerateTerrain(_key);

        var moistureGenerator = new MoistureGenerator(Data, _id);
        moistureGenerator.Generate(_key);

        var locationGenerator = new LocationGenerator(Data);
        locationGenerator.Generate(_key, _id);

        var regimeGen = new RegimeGenerator(Data, _id, _key);
        regimeGen.Generate();

        var peepGen = new PeepGenerator(_id, _key, Data);
        peepGen.Generate();
        
        
        return Data;
    }
}