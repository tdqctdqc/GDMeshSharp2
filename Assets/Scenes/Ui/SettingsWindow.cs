using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public class SettingsWindow : WindowDialog
{
    private SettingsWindow()
    {
        
    }
    public static SettingsWindow Get(ISettings settings)
    {
        var sw = SceneManager.Instance<SettingsWindow>();
        sw.Setup(settings);
        return sw;
    }
    public static SettingsWindow Get(MultiSettings multi)
    {
        var sw = SceneManager.Instance<SettingsWindow>();
        multi.Settings.ForEach(s => sw.Setup(s));
        return sw;
    }
    private void Setup(ISettings settings)
    {
        var tabs = (TabContainer) FindNode("TabContainer");
        var controls = SettingsControls.Construct(settings);
        controls.Name = settings.Name;
        tabs.AddChild(controls);
    }
}