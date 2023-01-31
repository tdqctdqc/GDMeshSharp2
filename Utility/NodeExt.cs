using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class NodeExt
{
    public static void ChildAndCenterOn(this Node2D parent, Control toCenter, Vector2 parentDim)
    {
        parent.AddChild(toCenter);
        toCenter.RectPosition = -parentDim / 2f;
    }
    public static Label CreateLabelAsChild(this Node parent, string text)
    {
        var label = new Label();
        label.Text = text;
        parent.AddChild(label);
        return label;
    }
    public static void AddToChildWithName(this Node self, Node toAdd, string childName)
    {
        self.FindNode(childName).AddChild(toAdd);
    }
}