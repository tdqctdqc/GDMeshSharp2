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
    public override void Produce(ItemWallet gains,
        EntityWallet<ResourceDeposit> depletions, Building p, float staffingRatio, Data data)
    {
        if (p.Model.Model() != this) throw new Exception();
        
        
        var deposit = p.Position.Poly(data).GetResourceDeposits(data)
            .First(d => d.Item.Model() == ProdItem);
        var depSize = deposit.Size;
        
        staffingRatio = Mathf.Clamp(staffingRatio, 0f, 1f);
        var ratio = GetProductionRatio(p, staffingRatio, data);
        var prod = Mathf.FloorToInt(ratio * ProductionCap);
        prod = Mathf.Min(Mathf.FloorToInt(depSize), prod);
        gains.Add(ProdItem, prod);
        var depletion = ProdItem.Attributes.Get<ExtractableAttribute>().GetDepletionFromProduction(deposit.Size, prod);
        depletions.Add(deposit, depletion);
    }
}
