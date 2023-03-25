using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GameUiTopBarToken : ButtonBarToken
{
    public static GameUiTopBarToken Get(bool host, Data data)
    {
        var g = new GameUiTopBarToken();
        Create<GameUiTopBarToken, HBoxContainer>(g);
        g.Setup(host,data);
        return g;
    }
    
    public void Setup(bool host, Data data)
    {
        AddWindowButton<EntityOverviewWindow>(Ui.Entities);
        AddWindowButton<ClientSettingsWindow>(Ui.ClientSettings);
        AddWindowButton<LoggerWindow>(Ui.Logger);
        
        var hostClientLabel = new Label();
        hostClientLabel.Text = host ? "Host" : "Client";
        Container.AddChildWithVSeparator(hostClientLabel);
        Container.AddChildWithVSeparator(TickDisplay.Create());
        Container.AddChildWithVSeparator(PlayerRegimeDisplay.Create(data));

        var resourceBar = new ItemBar();
        resourceBar.Setup(data);
        Container.AddChildWithVSeparator(resourceBar);
    }
}