using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class ExtractionBuildingModel : ProductionBuildingModel
{
    public ExtractionBuildingModel(Item prodItem, string name, bool fromDeposit, float buildCost) : base(prodItem, name, buildCost)
    {
        if (prodItem.Attributes.Has<ExtractableAttribute>() == false) throw new Exception();
    }
    public override void Produce(WorkProdConsumeProcedure proc, Building b, float staffingRatio, Data data)
    {
        if (b.Model.Model() != this) throw new Exception();
        staffingRatio = Mathf.Clamp(staffingRatio, 0f, 1f);
        var deposit = b.Position.Poly(data).GetResourceDeposits(data)
            .First(d => d.Item.Model() == ProdItem);
        var depSize = deposit.Size;
        var ratio = GetProductionRatio(b, staffingRatio, data);
        var prod = Mathf.FloorToInt(ratio * ProductionCap);
        prod = Mathf.Min(Mathf.FloorToInt(depSize), prod);
        var rId = b.Position.Poly(data).Regime.RefId;
        var depletion = ProdItem.Attributes.Get<ExtractableAttribute>()
            .GetDepletionFromProduction(deposit.Size, prod);
        
        proc.RegimeResourceGains[rId].Add(ProdItem, prod);
        proc.Depletions[rId].Add(deposit, depletion);
    }
}
