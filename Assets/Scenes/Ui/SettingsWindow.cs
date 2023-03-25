using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public class SettingsWindow : WindowDialog
{
    protected SettingsWindow()
    {
        RectSize = Vector2.One * 500f;
    }
    public static SettingsWindow Get(ISettings settings)
    {
        var w = new SettingsWindow();
        w.Setup(settings);
        return w;
    }
    public static SettingsWindow Get(MultiSettings multi)
    {
        var sw = SceneManager.Instance<SettingsWindow>();
        multi.Settings.ForEach(s => sw.Setup(s));
        return sw;
    }
    protected void Setup(ISettings settings)
    {
        var tabs = new TabContainer();
        tabs.RectSize = RectSize;
        AddChild(tabs);
        var controls = SettingsControls.Construct(settings);
        controls.Name = settings.Name;
        tabs.AddChild(controls);
    }
    protected void Setup(MultiSettings multi)
    {
        var tabs = new TabContainer();
        tabs.RectSize = RectSize;
        AddChild(tabs);
        foreach (var settings in multi.Settings)
        {
            var controls = SettingsControls.Construct(settings);
            controls.Name = settings.Name;
            tabs.AddChild(controls);
        }
    }
}