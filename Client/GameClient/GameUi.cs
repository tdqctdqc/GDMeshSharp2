using Godot;
using System;
using System.Collections.Generic;

public class GameUi : CanvasLayer
{
    private ButtonToken _entityOverviewBtn, _generate, _testSerialization;
    private EntityOverview _entityOverview;
    private MapDisplayOptionsUi _mapOptions;
    private Label _hostOrClient, _mousePos, _tick;
    
    public override void _Ready()
    {
        
    }

    public override void _Process(float delta)
    {
        

    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e is InputEventMouseMotion mm)
        {
            
        }
    }

    public void Setup(bool host, Data data, GameGraphics graphics, CameraController cam, GameClient client)
    {
        this.AssignChildNode(ref _hostOrClient, "HostOrClient");
        this.AssignChildNode(ref _tick, "Tick");
        ValueChangedNotice<GameClock, int>.Register(nameof(GameClock.Tick),
            n => _tick.Text = $"Tick: {n.NewVal}");
        
        this.AssignChildNode(ref _mousePos, "MousePos");
        _hostOrClient.Text = host ? "Host" : "Client";
        
        _entityOverviewBtn = ButtonToken.Get(this, "EntityOverviewBtn", () => _entityOverview.Popup_());
        _entityOverview = EntityOverview.Get(data);
        
        _testSerialization = ButtonToken.Get(this, "TestSerialization", () => client.Session.TestSerialization());
        
        AddChild(_entityOverview);

        this.AssignChildNode(ref _mapOptions, "MapDisplayOptionsUi");
        _mapOptions.Setup(graphics, cam, data);
    }
}
