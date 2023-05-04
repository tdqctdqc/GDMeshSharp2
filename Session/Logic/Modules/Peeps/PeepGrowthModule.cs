using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PeepGrowthModule : LogicModule
{
    public override void Calculate(Data data, Action<Message> queueMessage, 
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        var growths = new Dictionary<int, int>();
        foreach (var regime in data.Society.Regimes.Entities)
        {
            var foodDemanded = regime.History.DemandHistory[ItemManager.Food.Name].GetLatest();
            var foodProd = regime.History.ProdHistory[ItemManager.Food.Name].GetLatest();
            var surplusRatio = (float) foodProd / foodDemanded - 1f;
            if (surplusRatio > 0f)
            {
                HandleGrowth(regime, surplusRatio, growths, data);
            }
            else
            {
                HandleDecline(regime, -surplusRatio, growths, data);
            }
        }
        queueMessage(new PeepGrowthAndDeclineProcedure(growths));
    }

    private void HandleGrowth(Regime regime, float surplusRatio, Dictionary<int, int> growths,
        Data data)
    {
        var rules = data.BaseDomain.Rules;
        if (rules.MinSurplusRatioToGetGrowth > surplusRatio) return;
        
        var range = rules.MaxEffectiveSurplusRatio - rules.MinSurplusRatioToGetGrowth;
        if (range < 0) throw new Exception();
        
        var effectiveRatio = Mathf.Min(surplusRatio / range, rules.MaxEffectiveSurplusRatio);
        if (range < 0) throw new Exception();
        
        var peeps = regime.Polygons.Where(p => p.HasPeep(data))
            .Select(p => p.GetPeep(data));
        var numPeeps = peeps.Count();
        if (numPeeps == 0) return;
        
        var effect = rules.GrowthRateCeiling * effectiveRatio * peeps.Sum(p => p.Size());
        if (effect < 0) throw new Exception();

        var numPeepsToAffect = Mathf.CeilToInt(numPeeps / 10f);
        if (numPeepsToAffect < 0) throw new Exception();
        
        var peepsToAffect = peeps.GetDistinctRandomElements(numPeepsToAffect);

        var growthPerPeep = Mathf.CeilToInt(effect / numPeepsToAffect);
        if (growthPerPeep < 0) throw new Exception();
        for (var i = 0; i < peepsToAffect.Count; i++)
        {
            growths.Add(peepsToAffect[i].Id, growthPerPeep);
        }
    }
    private void HandleDecline(Regime regime, float surplusRatio, Dictionary<int, int> growths,
        Data data)
    {

    }
}
