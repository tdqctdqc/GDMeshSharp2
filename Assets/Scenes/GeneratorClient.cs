using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class GeneratorClient : Node, IClient
{
    private GeneratorSession _session;
    public CameraController Cam { get; private set; }
    private GameGraphics _graphics;
    private Label _progress;
    private bool _generating;
    private bool _generated;
    private MapDisplayOptionsUi _mapOptions;
    public CanvasLayer CanvasLayer => _canvasLayer;
    private CanvasLayer _canvasLayer;
    public ClientSettings Settings { get; private set; }
    public SettingsWindow GenSettingsWindow { get; private set; }
    public override void _Ready()
    {
        
    }

    public void Setup(GeneratorSession session)
    {
        _session = session;
        Settings = ClientSettings.Load();
        Cam = new CameraController();
        
        AddChild(Cam);
        Cam.Current = true;
        this.AssignChildNode(ref _progress, "Progress");
        this.AssignChildNode(ref _graphics, "Graphics");
        this.AssignChildNode(ref _mapOptions, "MapDisplayOptionsUi");
        this.AssignChildNode(ref _canvasLayer, "CanvasLayer");
        
        GenSettingsWindow = SettingsWindow.Get(_session.GenMultiSettings);
        _canvasLayer.AddChild(GenSettingsWindow);
        
        _mapOptions.Setup(_graphics, Cam, _session.Data);
        
        ButtonToken.FindButtonCreateToken(this, "Generate", () => PressedGenerate());
        ButtonToken.FindButtonCreateToken(this, "Done", GoToGameSession);
        ButtonToken.FindButtonCreateToken(this, "GenSettings", () => GenSettingsWindow.Popup_());
    }
    public void HandleInput(InputEvent e, float delta)
    {
    }
    

    public void GoToGameSession()
    {
        if (_generated)
        {
            Game.I.StartHostSession(_session.Data);
            QueueFree();
        }
    }
    public void Process(float delta)
    {
        if(_generating == false && _session.Succeeded) _graphics?.Process(delta, _session.Data);
    }
    private async void PressedGenerate()
    {
        if (_generating) return;
        _generating = true;
        await Task.Run(() => Generate()); 
        _generating = false;
        _generated = true;
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
        _session.GenerationFeedback += MonitorGeneration;
        _session.GenerationFailed += DisplayException;
        _session.Generate();
        if (_session.Succeeded)
        {
            _graphics.SetClient(this);
            _graphics.Setup(_session.Data);
        }
    }
}