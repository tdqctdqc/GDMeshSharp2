using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ButtonToken : Node
{
    private List<Action> _actions;
    public Button Button { get; private set; }
    public void Setup(Button button, params Action[] actions)
    {
        Button = button;
        _actions = actions.ToList();
        if(button.IsConnected("button_up", this, nameof(OnButtonUp)) == false)
            button.Connect("button_up", this, nameof(OnButtonUp));
        var p = GetParent();
        if (p != button)
        {
            p?.RemoveChild(this);
            button.AddChild(this);
        }
    }
    private void OnButtonUp()
    {
        for (int i = 0; i < _actions.Count; i++)
        {
            _actions[i].Invoke();
        }
    }
}