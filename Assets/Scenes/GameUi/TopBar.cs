using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TopBar : Control
{
    public void Setup(bool host, Data data, GameClient client)
    {
        var container = FindNode("Container");
        
        var entityOverviewBtn 
            = ButtonToken.CreateButton("EntityOverviewBtn", () => client.Ui.EntityOverviewWindow.Popup_());
        container.AddChildWithVSeparator(entityOverviewBtn);
        
        var settingsWindowBtn 
            = ButtonToken.CreateButton("SettingsWindowBtn", () => client.Ui.SettingsWindow.Popup_());
        container.AddChildWithVSeparator(settingsWindowBtn);

        
        var hostClientLabel = new Label();
        hostClientLabel.Text = host ? "Host" : "Client";
        container.AddChildWithVSeparator(hostClientLabel);
        container.AddChildWithVSeparator(TickDisplay.Create());
        container.AddChildWithVSeparator(PlayerRegimeDisplay.Create(data));

        var resourceBar = new ResourceBar();
        resourceBar.Setup(data);
        container.AddChildWithVSeparator(resourceBar);
    }

    
}