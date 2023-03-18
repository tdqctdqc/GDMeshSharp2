
using System;
using System.Collections.Generic;

public class ProductionResult : IResult
{
    public Dictionary<Regime, Dictionary<StratResource, float>> Production { get; private set; }
    
    public Dictionary<ResourceDeposit, float> Depletions { get; private set; }
    public ProductionResult()
    {
        Production = new Dictionary<Regime, Dictionary<StratResource, float>>();
        Depletions = new Dictionary<ResourceDeposit, float>();
    }

    public void AddResult(Regime regime, StratResource gaining, float gainAmt, ResourceDeposit depleted = null,
        float depleteAmt = 0f)
    {
        var gainDic = Production.GetOrAdd(regime, r => new Dictionary<StratResource, float>());
        gainDic.AddOrSum(gaining, gainAmt);
        if(depleted != null) Depletions.AddOrSum(depleted, -depleteAmt);
    }
    public void Poll(Action<Procedure> addProc, Action<Decision> addDec, Action<Update> addUpdate)
    {
        addProc(ProdProcedure.Create(this));
    }
}
