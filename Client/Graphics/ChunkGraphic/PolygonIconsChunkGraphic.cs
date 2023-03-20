
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolygonIconsChunkGraphic : Node2D
{
    private float _charWidth;
    private Font _defaultFont;
    private Action _drawNums;
    private PolygonIconsChunkGraphic()
    {}
    public PolygonIconsChunkGraphic(MapChunk chunk, Data data, 
        Func<MapPolygon, IEnumerable<IEnumerable<Icon>>> getIconGroups, int stackMax)
    {
        _defaultFont = (Font) GD.Load("res://Assets/Fonts/SmallFont.tres");
        _charWidth = _defaultFont.GetHeight();
        
        var iconDic = new Dictionary<Icon, List<Vector2>>();
        var nums = new List<int>();
        var numPoses = new List<Vector2>();
        foreach (var p in chunk.Polys)
        {
            var polyOffset = chunk.RelTo.GetOffsetTo(p.GetGraphicalCenterOffset(data) + p.Center, data);
            var iconGroups = getIconGroups(p);
            var yOffset = Vector2.Zero;
            foreach (var icons in iconGroups)
            {
                if (icons == null || icons.Count() == 0) continue;
                HandleIconGroup(icons, stackMax, polyOffset, yOffset, iconDic, nums, numPoses);
                yOffset += Vector2.Down * icons.First().Dimension.y;
            }
        }
        DrawMultiMeshes(iconDic);
    }

    
    private void HandleIconGroup(IEnumerable<Icon> icons, int stackMax, Vector2 polyOffset, Vector2 yOffset,
        Dictionary<Icon, List<Vector2>> iconDic, List<int> nums, List<Vector2> numPoses)
    {
        var iconCounts = icons.GetCounts();
        var xStep = 5f;
        var totalWidth = GetIconsWidth(iconCounts, stackMax, xStep);
        var shift = totalWidth * Vector2.Left / 2f;
        var xOffset = Vector2.Zero;
        foreach (var kvp in iconCounts)
        {
            var icon = kvp.Key;
            var count = kvp.Value;
            var iconsWidth = GetWidth(icon, count, stackMax, xStep);

            if (count <= stackMax)
            {
                for (var i = 0; i < count; i++)
                {
                    var iconPos = polyOffset + yOffset + xOffset + shift;
                    iconDic.AddOrUpdate(icon, iconPos);
                    xOffset += Vector2.Right * xStep;
                }
                xOffset += Vector2.Right * icon.Dimension.x;
            }
            else
            {
                var iconPos = polyOffset + yOffset + xOffset + shift;
                iconDic.AddOrUpdate(icon, iconPos);
                xOffset += Vector2.Right * (icon.Dimension.x / 2f);
                var countPos = polyOffset + yOffset + Vector2.Down * (icon.Dimension.y / 4f) + xOffset + shift;
                _drawNums += () => DrawString(_defaultFont, countPos, "x" + count.ToString(), Colors.Black);
                xOffset += Vector2.Right * _charWidth * (count.GetNumDigits() + 1);
            }
        }
    }

    private float GetIconsWidth(Dictionary<Icon, int> iconCounts, int stackMax, float xStep)
    {
        return iconCounts.Sum(kvp => GetWidth(kvp.Key, kvp.Value, stackMax, xStep));
    }

    private float GetWidth(Icon icon, int count, int stackMax, float xStep)
    {
        if (count <= stackMax) return icon.Dimension.x + (count - 1) * xStep;
        else return icon.Dimension.x + _charWidth * (count.GetNumDigits() + 1); //+1 for the x symbol
    }
    private void DrawMultiMeshes(Dictionary<Icon, List<Vector2>> iconDic)
    {
        foreach (var kvp in iconDic)
        {
            var icon = kvp.Key;
            var poses = kvp.Value;
            var mmi = new MultiMeshInstance2D();
            var mm = new MultiMesh();
            mm.Mesh = icon.Mesh;
            mm.InstanceCount = poses.Count;
            for (var i = 0; i < poses.Count; i++)
            {
                var transform = new Transform2D(Vector2.Right, Vector2.Up, poses[i]);
                mm.SetInstanceTransform2d(i, transform);
            }
            mmi.Texture = icon.BaseTexture;
            mmi.Multimesh = mm;
            AddChild(mmi);
        }
    }
    public override void _Draw()
    {
        if (_drawNums != null)
        {
            _drawNums.Invoke();
            _drawNums = null;
        }
    }
}
