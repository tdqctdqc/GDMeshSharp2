using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PromptWindow : WindowDialog
{
    public Action Satisfied { get; set; }
    public Action Dismissed { get; set; }
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
            var btnToken = ButtonToken.CreateToken(btn, 
                prompt.Actions[i],
                () =>
                {
                    Satisfied?.Invoke();
                    QueueFree();
                });
            btn.Text = prompt.ActionDescrs[i];
            _container.AddChild(btn);
        }

        Connect("popup_hide", this, nameof(Dismiss));
    }

    private void Dismiss()
    {
        Dismissed?.Invoke();
    }
}