using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeTerritory : Super<RegimeTerritory, GenPolygon>
{
    public Regime Regime { get; private set; }
    public RegimeTerritory(Regime regime) : base()
    {
        Regime = regime;
    }
    protected override IReadOnlyCollection<GenPolygon> GetSubNeighbors(GenPolygon sub)
    {
        return sub.GeoNeighbors.Refs;
    }
    protected override RegimeTerritory GetSubSuper(GenPolygon sub)
    {
        return sub.Regime != null ? sub.Regime.Territory : null;
    }
    protected override void SetSubSuper(GenPolygon sub, RegimeTerritory super)
    {
        //todo fix!!
    }
}