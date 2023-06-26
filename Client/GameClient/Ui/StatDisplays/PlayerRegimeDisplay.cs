
using Godot;

public class PlayerRegimeDisplay
{
    public static Node Create(Data data)
    {
        var hbox = new HBoxContainer();
        var regimeName = new Label();
        var player = data.BaseDomain.PlayerAux.LocalPlayer;
        SubscribedStatLabel.Construct<string>("Regime", regimeName, 
            () => data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity()?.Name, 
            data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank);
        
        var income = IconStatDisplay.Construct(Icon.Create("Income", Icon.AspectRatio._1x1, 25f), 
            data, 
            () =>
            {
                var locRegime = data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity();
                    if(locRegime == null) return 0;
                    return locRegime.Finance.GetIncome(locRegime, data);
            }, 
            data.Notices.Ticked.Blank);
        hbox.AddChild(regimeName);
        hbox.AddChild(income);
        return hbox;
    }
}
