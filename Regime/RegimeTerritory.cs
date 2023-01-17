using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeTerritory : Super<RegimeTerritory, GeoPolygon>
{
    public Regime Regime { get; private set; }
    public RegimeTerritory(Regime regime) : base()
    {
        Regime = regime;
    }
    protected override IReadOnlyCollection<GeoPolygon> GetSubNeighbors(GeoPolygon sub)
    {
        return sub.GeoNeighbors;
    }

    protected override RegimeTerritory GetSubSuper(GeoPolygon sub)
    {
        return sub.Regime != null ? sub.Regime.Territory : null;
    }


    protected override void SetSubSuper(GeoPolygon sub, RegimeTerritory super)
    {
        sub.SetRegime(super.Regime);
    }
}