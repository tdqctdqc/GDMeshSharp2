using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TopBar : Control
{
    public void Setup(bool host, Data data, GameClient client)
    {
        var container = FindNode("Container");
        var testSerializationBtn 
            = ButtonToken.CreateButton("TestSerialization", () => client.Session.TestSerialization());
        container.AddChildWithVSeparator(testSerializationBtn);
        
        var entityOverviewBtn 
            = ButtonToken.CreateButton("EntityOverviewBtn", () => client.Ui.EntityOverviewWindow.Popup_());
        container.AddChildWithVSeparator(entityOverviewBtn);
        
        var settingsWindowBtn 
            = ButtonToken.CreateButton("SettingsWindowBtn", () => client.Ui.SettingsWindow.Popup_());
        container.AddChildWithVSeparator(settingsWindowBtn);

        
        var hostClient = new Label();
        hostClient.Text = host ? "Host" : "Client";
        container.AddChildWithVSeparator(hostClient);
        container.AddChildWithVSeparator(TickDisplay.Create());
        container.AddChildWithVSeparator(PlayerRegimeDisplay.Create(data));

        var resourceBar = new ResourceBar();
        resourceBar.Setup(data);
        container.AddChildWithVSeparator(resourceBar);
    }

    
}