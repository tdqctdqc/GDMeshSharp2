using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PromptWindow : WindowDialog
{
    private Container _container;
    public void Setup(IPrompt prompt)
    {
        this.AssignChildNode(ref _container, "Container");
        _container.ClearChildren();
        var descrLabel = new Label();
        descrLabel.Text = prompt.Descr;
        _container.AddChild(descrLabel);
        for (var i = 0; i < prompt.Actions.Count; i++)
        {
            var btn = new Button();
            var btnToken = ButtonToken.CreateToken(btn, prompt.Actions[i], () => QueueFree());
            btn.Text = prompt.ActionDescrs[i];
            _container.AddChild(btn);
        }
    }
}