using Godot;
using System;
using System.Collections.Generic;

public class GameUi : Ui
{
    public PromptManager Prompts { get; private set; }
    public TooltipManager TooltipManager { get; private set; }
    public void Process(float delta, ICameraController cam, ClientWriteKey key)
    {
        TooltipManager.Process(delta, cam.GetMousePosInMapSpace());
        Prompts.Process(delta, key);    
    }

    public static GameUi Construct(GameClient client, bool host, Data data, MapGraphics graphics)
    {
        var ui = new GameUi(client);
        ui.Setup(host, data, client);
        return ui;
    }
    private GameUi() : base() 
    {
    }

    protected GameUi(IClient client) : base(client)
    {
        
    }

    public void Setup(bool host, Data data, GameClient client)
    {
        AddWindow(LoggerWindow.Get());
        AddWindow(ClientSettingsWindow.Get());
        AddWindow(EntityOverviewWindow.Get(data));
        AddWindow(SettingsWindow.Get(Game.I.Client.Settings));
        AddWindow(new RegimeOverviewWindow());

        var mapOptions = new MapDisplayOptionsUi();
        mapOptions.Setup(client.Graphics, data);
        mapOptions.RectPosition = Vector2.Down * 50f;
        AddChild(mapOptions);

        Prompts = new PromptManager(this, data);
        AddChild(GameUiTopBarToken.Get(host,client, data).Container);
        TooltipManager = new TooltipManager(data);
        AddChild(TooltipManager);
    }
}
