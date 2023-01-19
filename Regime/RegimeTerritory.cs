using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeTerritory : Super<RegimeTerritory, GenPolygon>
{
    public RegimeGen RegimeGen { get; private set; }
    public RegimeTerritory(RegimeGen regimeGen) : base()
    {
        RegimeGen = regimeGen;
    }
    protected override IReadOnlyCollection<GenPolygon> GetSubNeighbors(GenPolygon sub)
    {
        return sub.GeoNeighbors.Refs;
    }
    protected override RegimeTerritory GetSubSuper(GenPolygon sub)
    {
        return sub.RegimeGen != null ? sub.RegimeGen.Territory : null;
    }
    protected override void SetSubSuper(GenPolygon sub, RegimeTerritory super)
    {
        sub.Set(nameof(GenPolygon.RegimeGen), super.RegimeGen, new CreateWriteKey(null));
    }
}