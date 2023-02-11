using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GeneratorSession : Node, ISession
{
    RefFulfiller ISession.RefFulfiller => Data.RefFulfiller;
    public GenData Data => _worldGen.Data;
    IClient ISession.Client => Client;
    public GeneratorClient Client { get; private set; }
    private WorldGenerator _worldGen;
    
    public bool Generating { get; private set; }
    public GeneratorSession()
    {
        _worldGen = new WorldGenerator(new GenerationParameters(Vector2.Zero));
        Client = SceneManager.Instance<GeneratorClient>();
        Client.Setup(this);
        AddChild(Client);
    }
    
    public void Generate(int seed, GenerationParameters genParams, Action<string,string> monitor)
    {
        Generating = true;
        _worldGen = new WorldGenerator(genParams);
        Game.I.Random.Seed = (ulong) seed;
        _worldGen.Generate(monitor);
        Generating = false;
    }

    public override void _Process(float delta)
    {
        Client.ProcessPoly(delta);
    }
}