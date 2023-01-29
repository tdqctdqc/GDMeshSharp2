using Godot;
using System;
using System.Collections.Generic;

public class GameUi : CanvasLayer
{
    public static GameUi Get() 
        => (GameUi) ((PackedScene) GD.Load("res://Client/GameClient/GameUi.tscn")).Instance();
    
    private ButtonToken _entityOverviewBtn, _generate;
    private EntityOverview _entityOverview;
    private MapDisplayOptionsUi _mapOptions;
    private Label _hostOrClient;
    
    public override void _Ready()
    {
        
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e is InputEventMouseMotion mm)
        {
            var pos = mm.GlobalPosition;
            
        }
    }

    public void Setup(bool host, Data data, GameGraphics graphics)
    {
        _hostOrClient = (Label) FindNode("HostOrClient");
        _hostOrClient.Text = host ? "Host" : "Client";
        
        _entityOverviewBtn = ButtonToken.Get(this, "EntityOverviewBtn", () => _entityOverview.Popup_());
        _entityOverview = EntityOverview.Get(data);
        
        AddChild(_entityOverview);

        _mapOptions = (MapDisplayOptionsUi) FindNode("MapDisplayOptionsUi");
        _mapOptions.Setup(graphics);
    }
}
