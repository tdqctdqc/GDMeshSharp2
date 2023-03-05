using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GeneratorSession : Node, ISession
{
    RefFulfiller ISession.RefFulfiller => Data.RefFulfiller;
    public GenData Data => WorldGen.Data;
    IClient ISession.Client => Client;
    public Guid PlayerGuid => default;
    public GeneratorClient Client { get; private set; }
    public WorldGenerator WorldGen { get; private set; }
    public Action<DisplayableException> GenerationFailed { get; set; }
    public Action<string, string> GenerationFeedback { get; set; }
    public bool Generating { get; private set; }
    public bool Succeeded { get; private set; }
    public GeneratorSession()
    {
        WorldGen = new WorldGenerator(new GenerationParameters(Vector2.Zero));
        Client = SceneManager.Instance<GeneratorClient>();
        Client.Setup(this);
        AddChild(Client);
    }
    
    public bool Generate(int seed, GenerationParameters genParams)
    {
        Succeeded = false;
        Generating = true;
        WorldGen = new WorldGenerator(genParams);
        WorldGen.GenerationFailed += GenerationFailed;
        WorldGen.GenerationFeedback += GenerationFeedback;
        
        Game.I.Random.Seed = (ulong) seed;
        Succeeded = WorldGen.Generate();
        Generating = false;
        return Succeeded;
    }
    public override void _Process(float delta)
    {
        Client.Process(delta);
    }
}