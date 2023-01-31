using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ButtonContainer : Container
{
    private List<Button> _buttons;
    
    public ButtonContainer()
    {
        _buttons = new List<Button>();
    }
    public void AddButton(Action buttonAction, string buttonName)
    {
        var button = new Button();
        button.Text = buttonName;
        var token = ButtonToken.Get(button, buttonAction);
        AddChild(button);
        _buttons.Add(button);
    }

    public void Clear()
    {
        _buttons.ForEach(b => b.Free());
        _buttons.Clear();
    }
}