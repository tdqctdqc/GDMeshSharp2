using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ClientPlayerData
{
    public Guid LocalPlayerGuid { get; private set; }
    public TurnOrders Orders { get; private set; }
    public ClientPlayerData(Data data)
    {
        data.BaseDomain.PlayerAux.PlayerChangedRegime.Subscribe(a =>
        {
            var localPlayer = data.BaseDomain.PlayerAux.LocalPlayer;
            if (data.BaseDomain.PlayerAux.ByRegime.ContainsKey(a.NewVal.Entity()))
            {
                var player = data.BaseDomain.PlayerAux.ByRegime[a.NewVal.Entity()];
                if(player.PlayerGuid == localPlayer.PlayerGuid)
                {
                    PrepareBlankOrders(data);
                }
            }
        });
        
        data.Notices.Ticked.Subscribe(i => PrepareBlankOrders(data));
    }

    public void SetLocalPlayerGuid(Guid guid)
    {
        if (LocalPlayerGuid != default)
        {
            throw new Exception();
        }

        LocalPlayerGuid = guid;
    }
    public void PrepareBlankOrders(Data data)
    {
        var localPlayer = data.BaseDomain.PlayerAux.LocalPlayer;

        if (data.BaseDomain.GameClock.MajorTurn(data))
        {
            Orders = new MajorTurnOrders(data.BaseDomain.GameClock.Tick, localPlayer.Regime);
        }
        else
        {
            Orders = new MinorTurnOrders(data.BaseDomain.GameClock.Tick, localPlayer.Regime);
        }
    }
}
