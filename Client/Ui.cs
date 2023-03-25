using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Ui : CanvasLayer
{
    protected Dictionary<string, WindowDialog> _windows;
    public static string Logger = "Logger";
    public static string Entities = "Entities";
    public static string ClientSettings = "Settings";
    public static string GenSettings = "Generator Settings";
    protected Ui(IClient client)
    {
        _windows = new Dictionary<string, WindowDialog>();
        client.Requests.OpenWindowRequest.Subscribe(name => _windows[name].Popup_());
    }
    protected Ui()
    {
        
    }
    protected void AddWindow(WindowDialog window, string name)
    {
        _windows.Add(name, window);
        AddChild(window);
    }
}
