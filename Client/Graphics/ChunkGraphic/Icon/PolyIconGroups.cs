using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyIconGroups : Node2D
{
    public float Height { get; private set; }
    private static float _charHeight => _defaultTheme.DefaultFont.GetHeight();
    private static Theme _defaultTheme = (Theme)GD.Load("res://Assets/Themes/DefaultTheme.tres");
    private Action _drawNums;
    private List<List<Node2D>> _groups;
    private List<float> _zoomCutoffs;
    public PolyIconGroups(List<List<Icon>> groups, 
        List<List<string>> labels, 
        List<float> zoomCutoffs)
    {
        _groups = new List<List<Node2D>>();
        _zoomCutoffs = zoomCutoffs;
        var yMargin = 10f;
        Height = groups.Sum(g => g.Max(i => i.Dimension.y + yMargin));
        this.ClearChildren();
        var yOffset = Vector2.Zero;
        var yStart = Vector2.Up * Height / 4f;
        for (var i = 0; i < groups.Count; i++)
        {
            var groupHeight = groups[i].Max(g => g.Dimension.y);
            var groupOffset = yStart + yOffset;
            yOffset += Vector2.Down * groupHeight + yMargin * Vector2.Down;
            HandleIconGroup(groups[i], labels[i], groupOffset);
        }
    }

    private void HandleIconGroup(List<Icon> icons, List<string> labels, Vector2 yOffset)
    {
        var margin = 10f;
        var totalWidth = icons.Sum(i => i.Dimension.x);
        var shift = totalWidth * Vector2.Left / 2f;
        var xOffset = Vector2.Zero;
        var mis = new List<Node2D>();
        _groups.Add(mis);
        for (var i = 0; i < icons.Count; i++)
        {
            var icon = icons[i];
            var iconPos = xOffset + shift + yOffset;
            xOffset += Vector2.Right * (icon.Dimension.x + margin);
            var label = labels[i];
            var mi = icon.GetMeshInstance();
            mis.Add(mi);
            mi.Position = iconPos;
            AddChild(mi);
            var labelNode = NodeExt.CreateLabel(label);
            labelNode.Align = Label.AlignEnum.Center;
            labelNode.RectScale = new Vector2(1f, -1f);
            labelNode.Theme = _defaultTheme;
            labelNode.RectPosition = Vector2.Up * icon.Dimension.y / 2f + icon.Dimension.x / 4f * Vector2.Left;
            mi.AddChild(labelNode);
        }
    }

    public void DoScaling()
    {
        var zoom = Game.I.Client.Cam.ZoomOut;
        Scale = new Vector2(1, -1) * zoom;
        for (var i = 0; i < _groups.Count; i++)
        {
            var cutoff = _zoomCutoffs[i];
            if (zoom > cutoff)
            {
                _groups[i].ForEach(g => g.Visible = false);
            }
            else 
            {
                _groups[i].ForEach(g => g.Visible = true);
            }
        }
    }
}
