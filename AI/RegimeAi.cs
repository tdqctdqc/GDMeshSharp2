
using System.Collections.Generic;

public class RegimeAi
{
    public Regime Regime { get; private set; }
    public BudgetAI Budget { get; private set; }
    public RegimeAi(Regime regime, Data data)
    {
        Regime = regime;
        Budget = new BudgetAI(data, regime);
    }

    public MajorTurnOrders GetMajorTurnOrders(Data data)
    {
        var orders = new MajorTurnOrders();
        Budget.Calculate(data, orders);
        return orders; 
    }
    public MinorTurnOrders GetMinorTurnOrders(Data data)
    {
        var orders = new MinorTurnOrders();

        return orders; 
    }
}
