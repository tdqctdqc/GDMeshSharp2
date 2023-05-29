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
        try
        {
            await Task.Run(_session.Generate); 
        }
        catch (Exception e)
        {
            if (e is DisplayableException d)
            {
                DisplayException(d);
            }
            else
            {
                if (e is AggregateException a
                    && a.InnerExceptions.FirstOrDefault(i => i is DisplayableException) is DisplayableException da)
                {
                    DisplayException(da);
                }
                else
                {
                    GD.Print(e.Message);
                    GD.Print(e.StackTrace);
                    throw e;
                }
            }
        }
        
        _generating = false;
    }

    private void DisplayException(DisplayableException d)
    {
        var display = new Node2D();
        AddChild(display);
        GD.Print(d.StackTrace);
                
        var graphic = d.GetGraphic();
        display.AddChild(graphic);
        var cam = new DebugCameraController(graphic);
        cam.Current = true;

        display.AddChild(cam);
    }
}