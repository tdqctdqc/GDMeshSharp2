using Godot;
using System;

public class ExampleUi : Control
{
    public static ExampleUi Get() 
        => (ExampleUi) ((PackedScene) GD.Load("res://Session/Client/ExampleClient/ExampleUi/ExampleUi.tscn")).Instance();
    
    private Button _entityOverviewBtn;
    private EntityOverview _entityOverview;
    private Label _hostOrClient;
    public override void _Ready()
    {
        
    }

    public void Setup(bool host)
    {
        _hostOrClient = (Label) FindNode("HostOrClient");
        _hostOrClient.Text = host ? "Host" : "Client";
        _entityOverviewBtn = (Button) FindNode("EntityOverviewBtn");
        _entityOverviewBtn.Connect("button_up", this, nameof(ShowEntityOverview));
        _entityOverview = EntityOverview.Get();
        AddChild(_entityOverview);
    }

    public void ShowEntityOverview()
    {
        _entityOverview.Popup_();
    }
    
}
