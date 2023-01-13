using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ButtonContainer : Container
{
    public void AddButton(Action buttonAction, string buttonName)
    {
        var button = new Button();
        button.Text = buttonName;
        var token = new ButtonToken();
        token.Setup(button, buttonAction);
        AddChild(button);
    }
}