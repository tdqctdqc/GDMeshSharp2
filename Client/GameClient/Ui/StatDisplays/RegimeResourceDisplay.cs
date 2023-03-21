
using Godot;

public class RegimeResourceDisplay
{
    public static Node Create(Item sr, Regime regime, Data data)
    {
        float height = 30f;
        float width = 100f;
        var box = new HBoxContainer();
        box.RectMinSize = new Vector2(width, height);
        var label = new Label();
        var icon = sr.ResIcon.GetTextureRect(Vector2.One * height);
        icon.RectMinSize = icon.RectSize;
        box.AddChild(icon);
        box.AddChild(label);
        icon.RectScale = new Vector2(1f, -1f);
        
        SubscribedStatLabel.ConstructForEntityTrigger<Regime, float>(regime, 
            "", label, 
            r => r.Resources[sr], 
            data.Notices.FinishedFrame);
        return box;
    }
}