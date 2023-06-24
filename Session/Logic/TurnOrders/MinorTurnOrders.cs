using System;
using System.Collections.Generic;
using System.Linq;

public class MinorTurnOrders : TurnOrders
{
    public MinorTurnOrders(int tick, EntityRef<Regime> regime) 
        : base(tick, regime)
    {
    }
}
