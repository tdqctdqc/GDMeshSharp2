using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeWanderer : Wanderer
{
    public Regime Regime { get; private set; }
    public RegimeWanderer(Regime regime, MapPolygon seed, WandererPicker host) : base(seed, host)
    {
        Regime = regime;
    }

    public override bool MoveAndPick(WandererPicker host)
    {
        var canTake = ValidAdjacent.Intersect(host.NotTaken);
        var canTakeCount = canTake.Count();
        if (canTakeCount == 0) return false;
        if (Picked.Count < 4)
        {
            Pick(canTake.First(), host);
            return ValidAdjacent.Count != 0;
        }

        var pick = canTake
            .Where(a => a.Neighbors.Refs().Where(n => Picked.Contains(n)).Count() > 1)
            .OrderBy(a => a.Neighbors.Refs().Where(n => Picked.Contains(n)).Count())
            .FirstOrDefault();
        if (pick != null)
        {
            Pick(pick, host);
            return true;
        }

        return false;
    }


    protected override bool Valid(MapPolygon poly)
    {
        return poly.IsLand();
    }
}
