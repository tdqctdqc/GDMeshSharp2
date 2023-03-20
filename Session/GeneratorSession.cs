using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GeneratorSession : Node, ISession
{
    RefFulfiller ISession.RefFulfiller => Data.RefFulfiller;
    public GenData Data { get; private set; }
    IClient ISession.Client => Client;
    
    public GeneratorClient Client { get; private set; }
    public WorldGenerator WorldGen { get; private set; }
    public Action<DisplayableException> GenerationFailed { get; set; }
    public Action<string, string> GenerationFeedback { get; set; }
    public bool Generating { get; private set; }
    public bool Succeeded { get; private set; }
    public IServer Server { get; private set; }
    public GenerationSettings GenSettings { get; private set; }

    public GeneratorSession()
    {
        Server = new DummyServer();
        GenSettings = new GenerationSettings();
        Client = SceneManager.Instance<GeneratorClient>();
        Client.Setup(this);
        AddChild(Client);
    }
    
    public void Generate()
    {
        Succeeded = false;
        Generating = true;
        Data = new GenData(GenSettings);
        WorldGen = new WorldGenerator(this, Data);
        WorldGen.GenerationFailed += GenerationFailed;
        WorldGen.GenerationFeedback += GenerationFeedback;

        WorldGen.Generate();
        
        Succeeded = WorldGen.Failed == false;
        Game.I.Random.Seed = (ulong) GenSettings.Seed.Value;
        Generating = false;
    }
    public override void _Process(float delta)
    {
        Client.Process(delta);
    }
}