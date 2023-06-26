
using System;
using Godot;

public class RegimeItemDisplay : IconStatDisplay
{
    public Regime Regime => _data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity(); 
    public Item Item { get; private set; }
    private Data _data;

    public static RegimeItemDisplay Create(Item item, Data data)
    {
        return new RegimeItemDisplay(item, data);
    }

    private RegimeItemDisplay(Item item, Data data)
    : base(item.Icon, data, () =>
        {
            var r = data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity();
            return r != null ? r.Items[item] : 0;
        }, data.Notices.Ticked.Blank)
    {
    }

    private RegimeItemDisplay()
    {
    }
}