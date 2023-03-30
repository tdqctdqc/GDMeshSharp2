
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolygonIconsChunkGraphic : Node2D
{
    private List<PolyIconGroups> _groups;
    private PolygonIconsChunkGraphic()
    {}
    public PolygonIconsChunkGraphic(MapChunk chunk, Data data, 
        Func<MapPolygon, PolyIconGroups> getIconGroups)
    {
        _groups = new List<PolyIconGroups>();
        var yMargin = 10f * Vector2.Down;
        foreach (var p in chunk.Polys)
        {
            var polyOffset = chunk.RelTo.GetOffsetTo(p.GetGraphicalCenterOffset(data) + p.Center, data);
            var group = getIconGroups(p);
            group.Position = polyOffset;
            AddChild(group);
            _groups.Add(group);
        }
    }
    public override void _Process(float delta)
    {
        _groups.ForEach(g => g.DoScaling());
    }
    
    
}
