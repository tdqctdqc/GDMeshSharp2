using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TopBar : Control
{
    public void Setup(bool host, Data data, GameClient client)
    {
        var testSerializationBtn 
            = ButtonToken.CreateButton("TestSerialization", () => client.Session.TestSerialization());
        this.AddChildWithVSeparator(testSerializationBtn);
        
        var entityOverviewBtn 
            = ButtonToken.CreateButton("EntityOverviewBtn", () => client.Ui.EntityOverview.Popup_());
        this.AddChildWithVSeparator(entityOverviewBtn);

        
        var hostClient = new Label();
        hostClient.Text = host ? "Host" : "Client";
        this.AddChildWithVSeparator(hostClient);
        this.AddChildWithVSeparator(TickDisplay.Create());
        this.AddChildWithVSeparator(PlayerRegimeDisplay.Create(data));

        var resourceBar = new ResourceBar();
        resourceBar.Setup(data);
        this.AddChildWithVSeparator(resourceBar);
    }

    
}