using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class GeneratorUi : Ui
{
    private GeneratorSession _session;
    private bool _generating;
    private Label _progress;
    private MapDisplayOptionsUi _mapOptions;

    // public GeneratorSettingsWindow GenSettingsWindow { get; private set; }
    public static GeneratorUi Construct(IClient client, GeneratorSession session, MapGraphics graphics)
    {
        var ui = new GeneratorUi(client);
        ui.Setup(graphics, session);
        return ui;
    }
    private GeneratorUi() : base() 
    {
    }

    protected GeneratorUi(IClient client) : base(client)
    {
        
    }
    public void Setup(MapGraphics graphics, GeneratorSession session)
    {
        _session = session;
        var topBar = ButtonBarToken.Create<HBoxContainer>();
        topBar.AddButton("Generate", PressedGenerate);
        topBar.AddButton("Done", GoToGameSession);
        topBar.AddWindowButton<GeneratorSettingsWindow>(Ui.GenSettings);
        topBar.AddButton("Test Serialization", () => Game.I.Serializer.Test(session.Data));
        
        
        
        AddChild(topBar.Container); 

        
        var genSettingsWindow = GeneratorSettingsWindow.Get(_session.GenMultiSettings);
        AddWindow(genSettingsWindow);
        
        
        
        var sideBar = ButtonBarToken.Create<VBoxContainer>();
        AddChild(sideBar.Container);
        _progress = new Label();
        _progress.Text = "Progress";
        sideBar.Container.AddChild(_progress);
        _mapOptions = new MapDisplayOptionsUi();
        _mapOptions.Setup(graphics, _session.Data);
        sideBar.Container.RectPosition = Vector2.Down * 50f;
        sideBar.Container.AddChild(_mapOptions);
        AddWindow(new RegimeOverviewWindow());
    }
    public void Process(float delta)
    {
    }
    public void GoToGameSession()
    {
        if (_session.Generated)
        {
            Game.I.StartHostSession(_session.Data);
            QueueFree();
        }
    }
    private async void PressedGenerate()
    {
        if (_generating) return;
        _generating = true;
        await Task.Run(() => Generate()); 
        _generating = false;
    }
    private void MonitorGeneration(string tag, string report)
    {
        _progress.Text = tag + " " + report;
    }
    private void DisplayException(DisplayableException d)
    {
        AddChild(d.GetGraphic());
        GD.Print(d.StackTrace);
    }
    private void Generate()
    {
        _session.WorldGen.GenerationFeedback += MonitorGeneration;
        _session.WorldGen.GenerationFailed += DisplayException;
        _session.Generate();
        // if (_session.WorldGen.Failed == false)
        // {
        //     _graphics.Setup(_session.Data);
        // }
    }
}