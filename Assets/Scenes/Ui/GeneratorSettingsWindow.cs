using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GeneratorSettingsWindow : SettingsWindow
{
    public static GeneratorSettingsWindow Get(GenerationMultiSettings settings)
    {
        var w = new GeneratorSettingsWindow();
        w.Setup(settings);
        return w;
    }

    private void Doot()
    {
        GD.Print("doot");
    }
    private GeneratorSettingsWindow()
    {
        Connect("about_to_show", this, "Doot");
    }
}
