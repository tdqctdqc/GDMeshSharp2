using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class IconGroups : Node2D
{
    public float Height { get; private set; }
    private static float _charHeight => _defaultTheme.DefaultFont.GetHeight();
    private static Theme _defaultTheme = (Theme)GD.Load("res://Assets/Themes/DefaultTheme.tres");
    private Action _drawNums;
    private List<IIconGroupController> _groups;
    private List<List<Node2D>> _groupNodes;
    private List<List<Label>> _groupLabelNodes;
    
    private List<float> _zoomCutoffs;

    public IconGroups(List<IIconGroupController> groups)
    {
        _groups = groups;
        _groupNodes = new List<List<Node2D>>();
        _groupLabelNodes = new List<List<Label>>();
        _zoomCutoffs = groups.Select(g => g.ZoomCutoff).ToList();
        var icons = groups.Select(g => g.GetIcons()).ToList();
        var yMargin = 10f;
        var heights = icons.Select(ic => ic.Max(i => i.GetHeight())).ToList();
        Height = heights.Sum();
        this.ClearChildren();
        var yOffset = Vector2.Zero;
        var yStart = Vector2.Up * Height / 4f;
        for (var i = 0; i < groups.Count; i++)
        {
            var groupOffset = yStart + yOffset;
            yOffset += Vector2.Down * heights[i] + yMargin * Vector2.Down;
            HandleIconGroup(groups[i], icons[i],groupOffset);
        }
    }

    private IconGroups()
    {
    }

    private void HandleIconGroup(IIconGroupController group, List<Icon> icons, Vector2 yOffset)
    {
        var labels = group.GetLabels();
        var margin = 10f;
        var totalWidth = icons.Sum(i => i.Dimension.x);
        var shift = totalWidth * Vector2.Left / 2f;
        var xOffset = Vector2.Zero;
        var mis = new List<Node2D>();
        var labelNodes = new List<Label>();
        _groupLabelNodes.Add(labelNodes);
        _groupNodes.Add(mis);
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
            labelNodes.Add(labelNode);
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
            _groups[i].UpdateLabels(_groupLabelNodes[i]);
        }
        for (var i = 0; i < _groupNodes.Count; i++)
        {
            var cutoff = _zoomCutoffs[i];
            if (zoom > cutoff)
            {
                _groupNodes[i].ForEach(g => g.Visible = false);
            }
            else 
            {
                _groupNodes[i].ForEach(g => g.Visible = true);
            }
        }
    }
}
