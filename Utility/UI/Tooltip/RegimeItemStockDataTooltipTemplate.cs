using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeItemStockDataTooltipTemplate : DataTooltipTemplate<RegimeItemDisplay>
{
    public RegimeItemStockDataTooltipTemplate() : base()
    {
    }
    protected override List<Func<RegimeItemDisplay, Data, Control>> _fastGetters { get; }
        = new List<Func<RegimeItemDisplay, Data, Control>>
        {
            (t, d) => NodeExt.CreateLabel(t.Item.Name),
            (t, d) => NodeExt.CreateLabel("Stock: " + t.Regime?.Items[t.Item]),
            (t, d) => NodeExt.CreateLabel("Prod: " + t.Regime?.History.ProdHistory.GetLatest(t.Item.Name)),
            (t, d) => NodeExt.CreateLabel("Consumed: " + t.Regime?.History.ConsumptionHistory.GetLatest(t.Item.Name)),
            (t, d) => NodeExt.CreateLabel("Demanded: " + t.Regime?.History.DemandHistory.GetLatest(t.Item.Name)),
        };
    protected override List<Func<RegimeItemDisplay, Data, Control>> _slowGetters { get; }
        = new List<Func<RegimeItemDisplay, Data, Control>>
        {
        };
}
