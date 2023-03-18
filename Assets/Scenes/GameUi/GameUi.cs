using Godot;
using System;
using System.Collections.Generic;

public class GameUi : CanvasLayer
{
    public EntityOverview EntityOverview { get; private set; }
    private MapDisplayOptionsUi _mapOptions;
    private TopBar _topBar;
    private Label  _mousePos;
    public PromptManager Prompts { get; private set; }
    
    public override void _Ready()
    {
        
    }

    public void Process(float delta, ClientWriteKey key)
    {
        Prompts.Process(delta, key);    
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e is InputEventMouseMotion mm)
        {
            
        }
    }

    public void Setup(bool host, Data data, GameGraphics graphics, 
        CameraController cam, GameClient client)
    {
        this.AssignChildNode(ref _mousePos, "MousePos");
        EntityOverview = EntityOverview.Get(data);
        AddChild(EntityOverview);

        this.AssignChildNode(ref _mapOptions, "MapDisplayOptionsUi");
        _mapOptions.Setup(graphics, cam, data);

        Prompts = new PromptManager(this, data, client.Key);
        
        this.AssignChildNode(ref _topBar, "TopBar");
        _topBar.Setup(host, data, client);
    }
}
