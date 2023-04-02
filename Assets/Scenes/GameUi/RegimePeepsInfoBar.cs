using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;

public class RegimePeepsInfoBar : HBoxContainer
{
    private RefAction _update;
    public void Setup(Data data)
    {
        var sizeLabel = new Label();
        var deltaLabel = new Label();
        _update = new RefAction();
        data.Notices.Ticked.Subscribe(_update);
        SubscribedStatLabel.Construct<int>("Pop Size", sizeLabel,
            () => GetPeepCount(data),
            _update
        );
        AddChild(sizeLabel);
        SubscribedStatLabel.Construct<int>("Pop Growth", deltaLabel,
            () => GetPeepDelta(data),
            _update
        );
        AddChild(deltaLabel);
    }

    private int GetPeepCount(Data data)
    {
        var r = data.BaseDomain.PlayerAux.LocalPlayer.Regime;
        if (r.Empty() == false)
        {
            return r.Entity().GetPeeps(data).Sum(p => p.Size);
        }

        return 0;
    }

    private int GetPeepDelta(Data data)
    {
        var r = data.BaseDomain.PlayerAux.LocalPlayer.Regime;
        if (r.Empty() == false)
        {
            return r.Entity().History.PeepHistory.GetLatestDelta();
        }

        return 0;
    }
}
