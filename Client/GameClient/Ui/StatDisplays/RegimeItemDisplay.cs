
using System;
using Godot;

public class RegimeItemDisplay : HBoxContainer
{
    public Regime Regime => _data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity(); 
    public Item Item { get; private set; }
    private Data _data;

    public static RegimeItemDisplay Create(Item item, Data data)
    {
        return new RegimeItemDisplay(item, data);
    }

    private RegimeItemDisplay(Item item, Data data)
    {
        _data = data;
        Item = item;
        float height = 50f;
        float width = 100f;
        RectMinSize = new Vector2(width, height);
        var amount = new Label();
        var icon = item.Icon.GetTextureRect(Vector2.One * height);
        icon.RectMinSize = icon.RectSize;
        AddChild(icon);
        AddChild(amount);
        icon.RectScale = new Vector2(1f, -1f);
        
        //todo make so updates when player switches regime
        SubscribedStatLabel.Construct<int>(
            "", 
            amount,
            () =>
            {
                var r = data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity();
                return r != null ? r.Items[item] : 0;
            }, 
            data.Notices.Ticked.Blank
        );
        
        var template = new RegimeItemStockDataTooltipTemplate();
        var instance = new DataTooltipInstance<RegimeItemDisplay>(template, this);
        var tooltipToken = TooltipToken.Construct(instance, this, data);
    }

    private RegimeItemDisplay()
    {
    }
}