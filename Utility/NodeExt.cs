using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class NodeExt
{
    public static List<Node> GetChildList(this Node n)
    {
        var l = new List<Node>();
        var children = n.GetChildren();
        foreach (var child in children)
        {
            l.Add((Node)child);
        }

        return l;
    }
    public static void AssignChildNode<T>(this Node n, ref T node, string name) where T : Node
    {
        node = (T) n.FindNode(name);
        if (node == null) throw new Exception();
    }
    public static void ChildAndCenterOn(this Node2D parent, Control toCenter, Vector2 parentDim)
    {
        parent.AddChild(toCenter);
        toCenter.RectPosition = -parentDim / 2f;
    }

    public static Label CreateLabel(string text)
    {
        var l = new Label();
        l.Text = text;
        return l;
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
    
    public static bool Toggle(this Node2D n)
    {
        n.Visible = n.Visible == false;
        return n.Visible;
    }

    public static void ClearChildren(this Node n)
    {
        while (n.GetChildCount() > 0)
        {
            n.RemoveChild(n.GetChild(0));
        }
    }
    public static void AddChildWithVSeparator(this Node parent, Node n)
    {
        parent.AddChild(n);
        var s = new VSeparator();
        parent.AddChild(s);
    }
}