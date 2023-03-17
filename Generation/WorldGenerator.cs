using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class WorldGenerator
{
    public GenData Data { get; private set; }
    public bool Failed { get; private set; }
    private GenWriteKey _key;
    private Stopwatch _sw;
    private GeneratorSession _session;
    public Action<DisplayableException> GenerationFailed { get; set; }
    public Action<string, string> GenerationFeedback { get; set; }
    public WorldGenerator(GeneratorSession session, GenerationParameters genParams)
    {
        _session = session;
        Data = new GenData(genParams);
        _key = new GenWriteKey(Data, session);
        Data.Setup();
        _sw = new Stopwatch();
    }
    public GenReport Generate()
    {
        var report = new GenReport(GetType().Name);
        Failed = ! ExceptionCatcher.Try(() => GenerateInner(), GenerationFailed);
        return report;
    }

    private GenData GenerateInner()
    {
        var cellSize = 200f;
        var edgePointMargin = new Vector2(cellSize, cellSize);
        var dim = Data.GenParams.Dimensions;
        var id = _key.IdDispenser.GetID();
        PlanetInfo.Create(Data.GenParams.Dimensions, _key);
        GameClock.Create(_key);
        _sw.Start();

        var points = PointsGenerator
            .GenerateConstrainedSemiRegularPoints
                (Data.GenParams.Dimensions - edgePointMargin, cellSize, cellSize * .75f, false, true)
            .Select(v => v + edgePointMargin / 2f).ToList();
        
        RunGenerator(new PolygonGenerator(points, Data.GenParams.Dimensions, true, cellSize));

        // EdgeDisturber.DisturbEdges(Data.Planet.Polygons.Entities, 
        //     Data.Planet.PlanetInfo.Value.Dimensions, _key);
        
        RunGenerator(new GeologyGenerator());
        
        RunGenerator(new ResourceGenerator());
        
        RunGenerator(new MoistureGenerator());
        
        EdgeDisturber.SplitEdges(Data.Planet.Polygons.Entities, _key, 50f);
        GenerationFeedback?.Invoke("Edge split", "");
        
        RunGenerator(new PolyTriGenerator());
        Data.Events.SetPolyShapes?.Invoke();
        
        RunGenerator(new RegimeGenerator());
        
        RunGenerator(new LocationGenerator());
        
        RunGenerator(new BuildingGenerator());
        
        RunGenerator(new PeepGenerator());
        
        _sw.Stop();
        GD.Print("world gen time was " + _sw.Elapsed.TotalMilliseconds + "ms");
        return Data;
    }

    private void RunGenerator(Generator gen)
    {
        var r = gen.Generate(_key);
        GenerationFeedback?.Invoke(gen.GetType().Name, r.GetTimes());
    }
}