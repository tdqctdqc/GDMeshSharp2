using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Ui : CanvasLayer
{
    protected Dictionary<Type, WindowDialog> _windows;
    public static string Logger = "Logger";
    public static string Entities = "Entities";
    public static string ClientSettings = "Settings";
    public static string GenSettings = "Generator Settings";
    public static string RegimeOverview = "Regime Overview";
    protected Ui(IClient client)
    {
        _windows = new Dictionary<Type, WindowDialog>();
        client.Requests.OpenWindowRequest.Subscribe(type =>
        {
            _windows[type].Popup_();
            return _windows[type];
        });
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
