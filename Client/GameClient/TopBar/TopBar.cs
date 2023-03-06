using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TopBar : Control
{
    private Label _hostOrClient, _tick, _regime;
    private ButtonToken _entityOverviewBtn, _testSerialization;
    public void Setup(bool host, Data data, GameClient client)
    {
        _testSerialization = ButtonToken.Create(this, "TestSerialization", () => client.Session.TestSerialization());

        
        _entityOverviewBtn = ButtonToken.Create(this, "EntityOverviewBtn", () => client.Ui.EntityOverview.Popup_());
        
        this.AssignChildNode(ref _hostOrClient, "HostOrClient");
        _hostOrClient.Text = host ? "Host" : "Client";
        
        this.AssignChildNode(ref _tick, "Tick");
        ValueChangedHandler<GameClock, int>.RegisterForAll(nameof(GameClock.Tick),
            n => _tick.Text = $"Tick: {n.NewVal}");
        SetupRegimeLabel(data);
    }

    private void SetupRegimeLabel(Data data)
    {
        this.AssignChildNode(ref _regime, "Regime");
        var player = data.BaseDomain.Players.LocalPlayer;
        if (player == null) throw new Exception();
        PollingStatDisplay.Construct(player, nameof(Player.Regime), _regime, p => p.Regime, 1f);
    }
}