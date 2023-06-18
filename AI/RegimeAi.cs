
using System.Collections.Generic;

public class RegimeAi
{
    public Regime Regime { get; private set; }
    public BudgetAI Budget { get; private set; }
    public List<AiPriority> ProdConstructPriorities { get; private set; }
    public RegimeAi(Regime regime, Data data)
    {
        Regime = regime;
        Budget = new BudgetAI(data, regime);
        ProdConstructPriorities = new List<AiPriority>();
    }
    
}
