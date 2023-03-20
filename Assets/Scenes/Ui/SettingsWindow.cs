using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using gdMeshSharp2.Client.Settings;
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
    private void Setup(ISettings settings)
    {
        var container = (Container) FindNode("Container");
        foreach (var option in settings.Options)
        {
            SetupOption(option, container);
        }
    }
    private void SetupOption(ISettingsOption option, Container container)
    {
        var l = new Label();
        l.Text = option.Name + ":";
        container.AddChild(l);
        container.AddChild(option.GetControlInterface());
    }
}