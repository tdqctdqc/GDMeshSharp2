using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class WorldGenerator
{
    public GenData Data { get; private set; }
    private IDDispenser _id;
    private GenWriteKey _key;
    private Stopwatch _sw;
    private GenerationParameters _genParams;
    public WorldGenerator(GenerationParameters genParams)
    {
        _genParams = genParams;
        _id = new IDDispenser();
        Data = new GenData();
        Data.Setup();
        _key = new GenWriteKey(Data);
        _sw = new Stopwatch();
    }
    public GenData Generate(Action<string, string> generationCallback = null)
    {

        var cellSize = 200f;
        var edgePointMargin = new Vector2(cellSize, cellSize);

        var planetInfo = PlanetInfo.Create(_genParams.Dimensions, _id.GetID(), _key);
        
        _sw.Start();

        var points = PointsGenerator
            .GenerateConstrainedSemiRegularPoints
                (_genParams.Dimensions - edgePointMargin, cellSize, cellSize * .75f, false, true)
            .Select(v => v + edgePointMargin / 2f).ToList();
        generationCallback("Points", "");

        PolygonGenerator.GenerateMapPolygons
        (
            points, _genParams.Dimensions, true, cellSize,
            _id,
            _key
        );
        generationCallback("Polygons", "");
        

        // EdgeDisturber.DisturbEdges(Data.Planet.Polygons.Entities, 
        //     Data.Planet.PlanetInfo.Value.Dimensions, _key);
        // generationCallback("Edge disturb", "");
        
        Data.Events.FinalizedPolyShapes?.Invoke();

        
        
        var geologyGenerator = new GeologyGenerator(Data, _id);
        geologyGenerator.GenerateTerrain(_key);
        generationCallback("Geology", "");

        var moistureGenerator = new MoistureGenerator(Data, _id);
        moistureGenerator.Generate(_key);
        generationCallback("Moisture", "");


        var locationGenerator = new LocationGenerator(Data);
        locationGenerator.Generate(_key, _id);
        generationCallback("Locations", "");


        var regimeGen = new RegimeGenerator(Data, _id, _key);
        regimeGen.Generate();
        generationCallback("Regimes", "");


        var peepGen = new PeepGenerator(_id, _key, Data);
        peepGen.Generate();
        generationCallback("Peeps", "");

        
        
        return Data;
    }
}