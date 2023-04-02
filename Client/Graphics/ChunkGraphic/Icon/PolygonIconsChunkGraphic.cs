
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolygonIconsChunkGraphic : Node2D
{
    private MapChunk _chunk;
    private Dictionary<int, IconGroups> _groups;
    private Func<MapPolygon, IconGroups> _getIconGroups;
    private PolygonIconsChunkGraphic()
    {}

    private HashSet<int> _updatePolyCache;
    private RefAction<MapPolygon> _updatePoly;
    private Data _data;
    public PolygonIconsChunkGraphic(MapChunk chunk, Data data, 
        Func<MapPolygon, IconGroups> getIconGroups)
    {
        _updatePolyCache = new HashSet<int>();
        _data = data;
        _chunk = chunk;
        _getIconGroups = getIconGroups;
        _groups = new Dictionary<int, IconGroups>();
        var yMargin = 10f * Vector2.Down;
        foreach (var p in chunk.Polys)
        {
            var polyOffset = chunk.RelTo.GetOffsetTo(p.GetGraphicalCenterOffset(data) + p.Center, data);
            var group = getIconGroups(p);
            group.Position = polyOffset;
            AddChild(group);
            _groups.Add(p.Id, group);
        }

        _updatePoly = new RefAction<MapPolygon>();
        _updatePoly.Subscribe(poly =>
        {
            if (_groups.ContainsKey(poly.Id))
            {
                _updatePolyCache.Add(poly.Id);
            }
        });
        data.Society.BuildingAux.LaborersDelta.Subscribe(_updatePoly);
    }

    private void UpdatePolyIcons(MapPolygon p)
    {
        var oldGroup = _groups[p.Id];
        _groups.Remove(p.Id);
        var oldPos = oldGroup.Position;
        oldGroup.QueueFree();
        var newGroup = _getIconGroups(p);
        newGroup.Position = oldPos;
        AddChild(newGroup);
        _groups[p.Id] = newGroup;
    }
    public override void _Process(float delta)
    {
        foreach (var kvp in _groups)
        {
            kvp.Value.DoScaling();
        }

        var cache = _updatePolyCache;
        _updatePolyCache = new HashSet<int>();
        foreach (var id in cache)
        {
            UpdatePolyIcons(_data.Planet.Polygons[id]);
        }
    }

    public override void _Draw()
    {
        
        base._Draw();
    }

    public override void _ExitTree()
    {
        _updatePoly.EndSubscriptions();
        base._ExitTree();
    }
}
