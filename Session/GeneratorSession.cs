using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GeneratorSession : Node, IDataSession
{
    RefFulfiller ISession.RefFulfiller => Data.RefFulfiller;
    Data IDataSession.Data => Data;
    public GenData Data { get; private set; }
    IClient ISession.Client => Client;
    public GeneratorClient Client { get; private set; }
    public WorldGenerator WorldGen { get; private set; }
    public bool Generated { get; private set; } = false;
    private bool _generating = false;
    public IServer Server { get; private set; }
    public GenerationMultiSettings GenMultiSettings { get; private set; }

    public GeneratorSession()
    {
        GenMultiSettings = new GenerationMultiSettings();
    }

    public void Setup()
    {
        Server = new DummyServer();
        Data = new GenData(GenMultiSettings);
        WorldGen = new WorldGenerator(this, Data);
        Client = new GeneratorClient();
        Client.Setup(this);
        AddChild(Client);
    }
    
    public void Generate()
    {
        _generating = true;
        if (Generated)
        {
            Reset();
        }
        Game.I.Random.Seed = (ulong) GenMultiSettings.PlanetSettings.Seed.Value;
        WorldGen.Generate();
        Generated = true;
        _generating = false;
        Client.Graphics.Setup(Data);
    }

    private void Reset()
    {
        this.ClearChildren();
        Client = null;
        Game.I.SetSerializer();
        Server = new DummyServer();
        Data = new GenData(GenMultiSettings);
        WorldGen = new WorldGenerator(this, Data);
        Client = new GeneratorClient();
        Client.Setup(this);
        AddChild(Client);
    }
    public override void _Process(float delta)
    {
        if(_generating == false) Client?.Process(delta, false);
    }
}