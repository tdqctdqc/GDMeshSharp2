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
    public Action<DisplayableException> GenerationFailed { get; set; }
    public Action<string, string> GenerationFeedback { get; set; }
    public WorldGenerator(GenerationParameters genParams)
    {
        _genParams = genParams;
        _id = new IDDispenser();
        Data = new GenData();
        Data.Setup();
        _key = new GenWriteKey(Data);
        _sw = new Stopwatch();
    }
    public bool Generate()
    {
        try
        {
            GenerateInner();
        }
        catch (Exception e)
        {
            GenerationFeedback?.Invoke("GENERATION FAILED" + e.Message, "");
            if (e is DisplayableException i)
            {
                GenerationFailed?.Invoke(i);
            }
            else
            {
                throw;
            }
            
            return false;
        }

        return true;
    }

    private GenData GenerateInner()
    {
        var cellSize = 200f;
        var edgePointMargin = new Vector2(cellSize, cellSize);

        var planetInfo = PlanetInfo.Create(_genParams.Dimensions, _id.GetID(), _key);
        
        _sw.Start();

        var points = PointsGenerator
            .GenerateConstrainedSemiRegularPoints
                (_genParams.Dimensions - edgePointMargin, cellSize, cellSize * .75f, false, true)
            .Select(v => v + edgePointMargin / 2f).ToList();
        GenerationFeedback?.Invoke("Points", "");

        PolygonGenerator.GenerateMapPolygons
        (
            points, _genParams.Dimensions, true, cellSize,
            _id,
            _key
        );
        GenerationFeedback?.Invoke("Polygons", "");

        // EdgeDisturber.DisturbEdges(Data.Planet.Polygons.Entities, 
        //     Data.Planet.PlanetInfo.Value.Dimensions, _key);
        
        
        
        
        new GeologyGenerator(Data, _id).GenerateTerrain(_key);
        GenerationFeedback?.Invoke("Geology", "");

        new MoistureGenerator(Data, _id).Generate(_key);
        GenerationFeedback?.Invoke("Moisture", "");
        
        
        EdgeDisturber.SplitEdges(Data.Planet.Polygons.Entities, _key, 50f);
        GenerationFeedback?.Invoke("Edge split", "");
        
        
        new PolyTriGenerator().BuildTris(_key, _id);
        GD.Print("built tris");
        GenerationFeedback?.Invoke("Built tris", "");
        Data.Events.FinalizedPolyShapes?.Invoke();

        var locationGenerator = new LocationGenerator(Data);
        locationGenerator.Generate(_key, _id);
        GenerationFeedback?.Invoke("Locations", "");

        var regimeGen = new RegimeGenerator(Data, _id, _key);
        regimeGen.Generate();
        GenerationFeedback?.Invoke("Regimes", "");
        
        var peepGen = new PeepGenerator(_id, _key, Data);
        peepGen.Generate();
        GenerationFeedback?.Invoke("Peeps", "");
        _sw.Stop();
        GD.Print("world gen time was " + _sw.Elapsed.TotalMilliseconds + "ms");
        return Data;
    }
}