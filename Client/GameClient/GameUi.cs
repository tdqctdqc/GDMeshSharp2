using Godot;
using System;

public class GameUi : Control
{
    public static GameUi Get() 
        => (GameUi) ((PackedScene) GD.Load("res://Client/GameClient/GameUi.tscn")).Instance();
    
    private ButtonToken _entityOverviewBtn, _generate;
    private EntityOverview _entityOverview;
    private Label _hostOrClient;
    public override void _Ready()
    {
        
    }

    public void Setup(bool host, Data data)
    {
        _hostOrClient = (Label) FindNode("HostOrClient");
        _hostOrClient.Text = host ? "Host" : "Client";
        
        _entityOverviewBtn = ButtonToken.Get(this, "EntityOverviewBtn", () => _entityOverview.Popup_());
        _entityOverview = EntityOverview.Get(data);
        
        AddChild(_entityOverview);
    }
}
