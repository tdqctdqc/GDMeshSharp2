using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GameUiTopBarToken : ButtonBarToken
{
    public static GameUiTopBarToken Get(bool host, GameClient client, Data data)
    {
        var g = new GameUiTopBarToken();
        Create<GameUiTopBarToken, HBoxContainer>(g);
        g.Setup(host,client, data);
        return g;
    }
    
    public void Setup(bool host, GameClient client, Data data)
    {
        AddWindowButton<EntityOverviewWindow>(Ui.Entities);
        AddWindowButton<ClientSettingsWindow>(Ui.ClientSettings);
        AddWindowButton<LoggerWindow>(Ui.Logger);
        AddButton("Submit Turn", () =>
        {
            var c = SubmitTurnCommand.Construct(data.ClientPlayerData.Orders);
            Game.I.Client.Requests.QueueCommand.Invoke(c);
        });
        
        var hostClientLabel = new Label();
        hostClientLabel.Text = host ? "Host" : "Client";
        Container.AddChildWithVSeparator(hostClientLabel);
        Container.AddChildWithVSeparator(TickDisplay.Create(data));
        Container.AddChildWithVSeparator(PlayerRegimeDisplay.Create(data));

        var itemBar = new ItemBar();
        itemBar.Setup(data);
        Container.AddChildWithVSeparator(itemBar);

        var peepsInfo = new RegimePeepsInfoBar();
        peepsInfo.Setup(data);
        Container.AddChildWithVSeparator(peepsInfo);
        
        
    }
}