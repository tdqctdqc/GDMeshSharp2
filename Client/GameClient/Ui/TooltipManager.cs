using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Godot;

public class TooltipManager : Control
{
    private TooltipPanel _panel;
    private ITooltipInstance _currInstance;
    private Vector2 _offsetFromMouse = new Vector2(20f, 20f);
    private Data _data;
    public TooltipManager(Data data)
    {
        _data = data;
        _panel = new TooltipPanel();
        AddChild(_panel);
        _panel.Visible = false;
    }

    private TooltipManager()
    {
        
    }
    public void Process(float delta, Vector2 mousePosInMapSpace)
    {
        _panel.Move(GetLocalMousePosition() + _offsetFromMouse);
    }

    public void PromptTooltip<T>(DataTooltipInstance<T> instance)
    {
        _panel.Visible = true;
        _panel.Setup(instance, _data);
        _currInstance = instance;
    }

    public void HideTooltip<T>(DataTooltipInstance<T> instance)
    {
        if (_currInstance == instance)
        {
            _panel.Visible = false;
            _currInstance = null;
        }
    }
}