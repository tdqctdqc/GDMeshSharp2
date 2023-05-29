
using System.Collections.Generic;

public class RegimeAi
{
    public Regime Regime { get; private set; }
    public ConstructionAI Construction { get; private set; }
    public List<AiPriority> ProdConstructPriorities { get; private set; }
    public RegimeAi(Regime regime, Data data)
    {
        Regime = regime;
        Construction = new ConstructionAI(data, regime);
        ProdConstructPriorities = new List<AiPriority>();
    }
    
}