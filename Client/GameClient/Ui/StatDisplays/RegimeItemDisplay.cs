
using System;
using Godot;

public class RegimeItemDisplay : HBoxContainer
{
    public Regime Regime { get; private set; }
    public Item Item { get; private set; }
    public static RegimeItemDisplay Create(Item item, Regime regime, Data data)
    {
        return new RegimeItemDisplay(item, regime, data);
    }

    private RegimeItemDisplay(Item item, Regime regime, Data data)
    {
        Regime = regime;
        Item = item;
        float height = 50f;
        float width = 100f;
        RectMinSize = new Vector2(width, height);
        var amount = new Label();
        var icon = item.ResIcon.GetTextureRect(Vector2.One * height);
        icon.RectMinSize = icon.RectSize;
        AddChild(icon);
        AddChild(amount);
        icon.RectScale = new Vector2(1f, -1f);
        
        //todo make so updates when player switches regime
        SubscribedStatLabel.ConstructForEntityTrigger<Regime, float>(
            regime, 
            "", 
            amount, 
            r => r.Items[item], 
            data.Notices.Ticked);
        
        var template = new RegimeItemStockDataTooltipTemplate();
        var instance = new DataTooltipInstance<RegimeItemDisplay>(template, this);
        var tooltipToken = TooltipToken.Construct(instance, this, data);
    }

    private RegimeItemDisplay()
    {
    }
}