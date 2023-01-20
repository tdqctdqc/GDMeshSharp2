using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeGenerator
{
    private WorldData _data;

    public RegimeGenerator(WorldData data)
    {
        _data = data;
    }

    public void Generate()
    {
        GenerateRegimes();
    }

    private void GenerateRegimes()
    {
        var polysPerRegime = 20;

        _data.LandSea.Landmasses.ForEach(lm =>
        {
            var landmassRegimes = Mathf.CeilToInt(lm.Count / polysPerRegime);
            var seeds = lm.GetNRandomElements(landmassRegimes);
            var regimes = new List<Regime>();
            for (var i = 0; i < seeds.Count; i++)
            {
                var prim = ColorsExt.GetRandomColor();
                var sec = prim.Inverted();
                var regime = new Regime(prim, sec, seeds[i]);
                regimes.Add(regime);
            }
            GenerationUtility.PickInTurn(lm.Where(p => p.Regime == null), 
                regimes, r => r.Territory.NeighboringSubs, (r, p) => r.Territory.AddSub(p));
        });
    }
}