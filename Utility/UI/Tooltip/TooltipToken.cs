using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TooltipToken : Node
{
    private IDataTooltipTemplate _template;
    private IClient _client;
    private Action _spawnTooltip;
    private Action _despawnTooltip;
    public static TooltipToken Construct<T>(DataTooltipInstance<T> instance, Control control, Data data)
    {
        var token = new TooltipToken(control,
            () => Game.I.Client.TooltipManager.PromptTooltip<T>(instance),
            () => Game.I.Client.TooltipManager.HideTooltip<T>(instance));
        return token;
    }
    
    private TooltipToken(Control control, Action spawnTooltip, Action despawnTooltip)
    {
        _despawnTooltip = despawnTooltip;
        _spawnTooltip = spawnTooltip;
        control.Connect("mouse_entered", this, nameof(MouseEnter));
        control.Connect("mouse_exited", this, nameof(MouseExit));
        control.AddChild(this);
    }

    private TooltipToken()
    {
    }

    private void MouseEnter()
    {
        _spawnTooltip();
    }

    private void MouseExit()
    {
        _despawnTooltip();
    }
}
