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
    public GenerationMultiSettings GenMultiSettings { get; private set; }

    public GeneratorSession()
    {
        
    }

    public void Setup()
    {
        Server = new DummyServer();
        GenMultiSettings = new GenerationMultiSettings();
        Data = new GenData(GenMultiSettings);
        Client = new GeneratorClient();
        Client.Setup(this);
        AddChild(Client);
    }
    
    public void Generate()
    {
        Succeeded = false;
        Generating = true;
        WorldGen = new WorldGenerator(this, Data);
        WorldGen.GenerationFailed += GenerationFailed;
        WorldGen.GenerationFeedback += GenerationFeedback;

        WorldGen.Generate();
        
        Succeeded = WorldGen.Failed == false;
        Game.I.Random.Seed = (ulong) GenMultiSettings.PlanetSettings.Seed.Value;
        Generating = false;
    }
    public override void _Process(float delta)
    {
        Client.Process(delta);
    }
}