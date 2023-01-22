using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeTerritory : Super<RegimeTerritory, MapPolygon>
{
    public Regime Regime { get; private set; }
    private Data _data;
    public RegimeTerritory(Regime regime, Data data) : base()
    {
        _data = data;
        Regime = regime;
    }

    private RegimeTerritory GetTerritory(Regime r)
    {
        return _data.Society.Regimes.Territories[r];
    }
    protected override IReadOnlyCollection<MapPolygon> GetSubNeighbors(MapPolygon sub)
    {
        return sub.Neighbors.Refs();
    }
    protected override RegimeTerritory GetSubSuper(MapPolygon sub)
    {
        var r = sub.Regime;
        if (r == null) return null;
        return GetTerritory(r.Ref());
    }
    protected override void SetSubSuper(MapPolygon sub, RegimeTerritory super)
    {
        var prevR = sub.Regime.Ref();
        if (prevR != null)
        {
            var prevTerr = GetTerritory(prevR);
            prevTerr.RemoveSub(sub);
        }
    }
}