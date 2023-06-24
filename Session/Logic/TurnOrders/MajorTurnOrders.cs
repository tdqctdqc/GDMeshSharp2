using System;
using System.Collections.Generic;
using System.Linq;

public class MajorTurnOrders : TurnOrders
{
    public StartConstructionsOrders StartConstructions { get; private set; }


    public MajorTurnOrders(int tick, EntityRef<Regime> regime) : base(tick, regime)
    {
        StartConstructions = StartConstructionsOrders.Construct();
    }
}
