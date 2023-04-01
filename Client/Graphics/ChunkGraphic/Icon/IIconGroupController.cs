using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IIconGroupController
{
    List<Icon> GetIcons();
    List<string> GetLabels();
    float ZoomCutoff { get; }
    void UpdateLabels(List<Label> labels);
}
