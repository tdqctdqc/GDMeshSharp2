using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GeneratorSession : Node, ISession
{
    RefFulfiller ISession.RefFulfiller => Data.RefFulfiller;
    public GenData Data => WorldGen.Data;
    IClient ISession.Client => Client;
    public GeneratorClient Client { get; private set; }
    public WorldGenerator WorldGen { get; private set; }
    public Action<DisplayableException> GenerationFailed { get; set; }
    public Action<string, string> GenerationFeedback { get; set; }
    public bool Generating { get; private set; }
    public GeneratorSession()
    {
        WorldGen = new WorldGenerator(new GenerationParameters(Vector2.Zero));
        Client = SceneManager.Instance<GeneratorClient>();
        Client.Setup(this);
        AddChild(Client);
    }
    
    public bool Generate(int seed, GenerationParameters genParams)
    {
        Generating = true;
        WorldGen = new WorldGenerator(genParams);
        WorldGen.GenerationFailed += GenerationFailed;
        WorldGen.GenerationFeedback += GenerationFeedback;
        
        Game.I.Random.Seed = (ulong) seed;
        var success = WorldGen.Generate();
        Generating = false;
        return success;
    }
    public override void _Process(float delta)
    {
        Client.ProcessPoly(delta);
    }
}