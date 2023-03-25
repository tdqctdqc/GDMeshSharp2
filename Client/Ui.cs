using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Ui : CanvasLayer
{
    protected Dictionary<Type, WindowDialog> _windows;

    protected Ui(IClient client)
    {
        _windows = new Dictionary<Type, WindowDialog>();
        client.Requests.OpenWindowType += t => _windows[t].Popup_();
    }
    protected Ui()
    {
        
    }
    protected void AddWindow(WindowDialog window)
    {
        _windows.Add(window.GetType(), window);
        AddChild(window);
    }
}
