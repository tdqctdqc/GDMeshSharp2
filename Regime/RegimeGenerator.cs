using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeGenerator
{
    private WorldData _data;
    private IDDispenser _id;
    private CreateWriteKey _key;
    public RegimeGenerator(WorldData data, IDDispenser id, CreateWriteKey key)
    {
        _id = id;
        _key = key;
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
            for (var i = 0; i < seeds.Count; i++)
            {
                var prim = ColorsExt.GetRandomColor();
                var sec = prim.Inverted();
                var regime = new Regime(_id.GetID(), _key, prim, sec, seeds[i]);
                _data.AddEntity(regime, typeof(SocietyDomain), _key);
            }
            GenerationUtility.PickInTurn(lm.Where(p => p.Regime == null), 
                _data.SocietyDomain.Regimes.Entities, r => r.Territory.NeighboringSubs, (r, p) =>
                {
                    r.Territory.AddSub(p);
                    p.Set(nameof(GenPolygon.Regime), r, _key);
                });
        });
    }
}