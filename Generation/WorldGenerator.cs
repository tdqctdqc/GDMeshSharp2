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
        Game.I.Random.Seed = (ulong)_session.GenMultiSettings.PlanetSettings.Seed.Value;
        var report = new GenReport(GetType().Name);
        Failed = ! ExceptionCatcher.Try(() => GenerateInner(report), GenerationFailed);
        return report;
    }

    private GenData GenerateInner(GenReport r)
    {
        _sw.Start();
        var polySize = 200f;
        var edgePointMargin = new Vector2(polySize, polySize);
        var dim = Data.GenMultiSettings.Dimensions;
        var id = _key.IdDispenser.GetID();
        GameClock.Create(_key);
        PlanetInfo.Create(Data.GenMultiSettings.Dimensions, _key);
        RuleVars.CreateDefault(_key);
        CurrentConstruction.Create(_key);
        
        var points = PointsGenerator
            .GenerateConstrainedSemiRegularPoints
                (Data.GenMultiSettings.Dimensions - edgePointMargin, polySize, polySize * .75f, false, true)
            .Select(v => v + edgePointMargin / 2f).ToList();
        
        foreach (var p in points)
        {
            if (p != p.Intify()) throw new Exception("not int point");
            if (p.x < 0 || p.x > dim.x || p.y < 0 || p.y > dim.y) throw new Exception("point out of bounds");
        }

        RunGenerator(new PolygonGenerator(points, Data.GenMultiSettings.Dimensions, true, polySize));
        
        RunGenerator(new GeologyGenerator());
        
        RunGenerator(new ResourceGenerator());
        
        EdgeDisturber.SplitEdges(Data.Planet.Polygons.Entities, _key, 
            Data.GenMultiSettings.PlanetSettings.PreferredMinPolyEdgeLength.Value);

        RunGenerator(new MoistureGenerator());
        
        RunGenerator(new PolyTriGenerator());
        
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
        try
        {
            var r = gen.Generate(_key);
            GenerationFeedback?.Invoke(gen.GetType().Name, r.GetTimes());
        }
        catch (Exception e)
        {
            if (e is DisplayableException d)
            {
                GenerationFailed.Invoke(d);
                return;
            }
            else
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
    }
}