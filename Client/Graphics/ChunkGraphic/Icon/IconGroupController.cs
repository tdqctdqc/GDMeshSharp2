using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class IconGroupController<T> : IIconGroupController
{
    public List<T> Elements { get; private set; }
    public Func<T, string> GetLabel { get; private set; }
    public Func<T, Icon> GetIcon { get; private set; }
    public float ZoomCutoff { get; private set; }

    public IconGroupController(List<T> elements, Func<T, string> getLabel, Func<T, Icon> getIcon, float zoomCutoff)
    {
        Elements = elements;
        GetLabel = getLabel;
        GetIcon = getIcon;
        ZoomCutoff = zoomCutoff;
    }

    public List<Icon> GetIcons()
    {
        return Elements.Select(GetIcon).ToList();
    }
    public List<string> GetLabels()
    {
        return Elements.Select(GetLabel).ToList();
    }
    public void UpdateLabels(List<Label> labels)
    {
        for (var i = 0; i < labels.Count; i++)
        {
            labels[i].Text = GetLabel(Elements[i]);
        }
    }
    
}
