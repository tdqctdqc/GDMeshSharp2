using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolygonSuper : Super<PolygonSuper, MapPolygon>
{
    public int Id { get; private set; }
    public Color Color { get; private set; }
    public MapPolygon Seed { get; private set; }
    private Dictionary<MapPolygon, PolygonSuper> _polySupers;

    public PolygonSuper(MapPolygon seed,Dictionary<MapPolygon, PolygonSuper> polySupers, int id) : base()
    {
        Color = ColorsExt.GetRandomColor();
        Seed = seed;
        _polySupers = polySupers;
        Id = id;
        AddSub(Seed);
    }

    protected override IReadOnlyCollection<MapPolygon> GetSubNeighbors(MapPolygon sub)
    {
        return sub.Neighbors.Refs();
    }

    protected override PolygonSuper GetSubSuper(MapPolygon sub)
    {
        return _polySupers.ContainsKey(sub) ? _polySupers[sub] : null;
    }

    protected override void SetSubSuper(MapPolygon sub, PolygonSuper super)
    {
        _polySupers[sub] = super;
    }
}