using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LoggerWindow : WindowDialog
{
    private Container _container;

    public static LoggerWindow Get()
    {
        return SceneManager.Instance<LoggerWindow>();
    }

    private LoggerWindow()
    {
        Connect("about_to_show", this, nameof(Draw));
    }
    public override void _Ready()
    {
        _container = (Container) FindNode("Container");
    }

    private void Draw()
    {
        _container.ClearChildren();
        foreach (var kvp in Game.I.Logger.Logs)
        {
            AddTab(kvp.Key, kvp.Value);
        }
    }

    private void AddTab(LogType lt, List<string> msgs)
    {
        var name = Enum.GetName(typeof(LogType), lt);
        var vbox = new VBoxContainer();
        vbox.Name = name;
        _container.AddChild(vbox);
        vbox.RectSize = _container.RectSize;
        for (var i = 0; i < msgs.Count; i++)
        {
            vbox.AddChild(NodeExt.CreateLabel(msgs[i]));
        }
    }
}