using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class IconStatDisplay : HBoxContainer
{
    public static IconStatDisplay Construct(Icon icon, Data data, Func<int> getAmt, RefAction trigger)
    {
        return new IconStatDisplay(icon, data, getAmt, trigger);
    }
    protected IconStatDisplay(Icon icon, Data data, Func<int> getAmt, RefAction trigger)
    {
        float height = 50f;
        float width = 100f;
        RectMinSize = new Vector2(width, height);
        var amount = new Label();
        var iconRect = icon.GetTextureRect(Vector2.One * height);
        iconRect.RectMinSize = iconRect.RectSize;
        AddChild(iconRect);
        AddChild(amount);
        iconRect.RectScale = new Vector2(1f, -1f);
        
        SubscribedStatLabel.Construct<int>(
            "", 
            amount,
            getAmt, 
            trigger
        );
    }

    protected IconStatDisplay()
    {
    }
}
