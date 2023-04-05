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
    public WorldGenerator(GeneratorSession session, GenData data)
    {
        _session = session;
        Data = data;
        _key = new GenWriteKey(Data, session);
        Data.Setup();
        _sw = new Stopwatch();
    }
    public GenReport Generate()
    {
        var report = new GenReport(GetType().Name);
        Failed = ! ExceptionCatcher.Try(() => GenerateInner(report), GenerationFailed);
        return report;
    }

    private GenData GenerateInner(GenReport r)
    {
        var polySize = 200f;
        var edgePointMargin = new Vector2(polySize, polySize);
        var dim = Data.GenMultiSettings.Dimensions;
        var id = _key.IdDispenser.GetID();
        GameClock.Create(_key);
        PlanetInfo.Create(Data.GenMultiSettings.Dimensions, _key);
        RuleVars.CreateDefault(_key);
        CurrentConstruction.Create(_key);
        _sw.Start();
        
        var points = PointsGenerator
            .GenerateConstrainedSemiRegularPoints
                (Data.GenMultiSettings.Dimensions - edgePointMargin, polySize, polySize * .75f, false, true)
            .Select(v => v + edgePointMargin / 2f).ToList();

        RunGenerator(new PolygonGenerator(points, Data.GenMultiSettings.Dimensions, true, polySize));
        
        RunGenerator(new GeologyGenerator());
        
        RunGenerator(new ResourceGenerator());
        
        RunGenerator(new MoistureGenerator());
        
        var sw1 = new Stopwatch();
        sw1.Start();
        EdgeDisturber.SplitEdges(Data.Planet.Polygons.Entities, _key, 
            Data.GenMultiSettings.PlanetSettings.PreferredMinPolyEdgeLength.Value);
        sw1.Stop();
        GD.Print("split edge time " + sw1.Elapsed.TotalMilliseconds);

        GenerationFeedback?.Invoke("Edge split", "");
        
        RunGenerator(new PolyTriGenerator());

        var sw2 = new Stopwatch();
        sw2.Start();
        Data.Notices.SetPolyShapes.Invoke();
        sw2.Stop();
        GD.Print("set poly shape time " + sw2.Elapsed.TotalMilliseconds);
        
        RunGenerator(new RegimeGenerator());
        
        RunGenerator(new LocationGenerator());
        
        RunGenerator(new RoadGenerator());
        
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